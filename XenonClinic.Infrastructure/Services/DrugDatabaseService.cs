using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Drug Database with RxNorm integration
/// </summary>
public class DrugDatabaseService : IDrugDatabaseService
{
    private readonly ClinicDbContext _context;
    private readonly ILogger<DrugDatabaseService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private const string RxNormApiBase = "https://rxnav.nlm.nih.gov/REST";
    private const string OpenFDAApiBase = "https://api.fda.gov/drug";
    private const string CacheKeyPrefix = "Drug_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public DrugDatabaseService(
        ClinicDbContext context,
        ILogger<DrugDatabaseService> logger,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("DrugDatabase");
        _cache = cache;
    }

    #region Drug Search

    public async Task<DrugSearchResultDto> SearchDrugsAsync(DrugSearchRequestDto request)
    {
        var results = new List<DrugSummaryDto>();

        try
        {
            switch (request.SearchType)
            {
                case DrugSearchTypeDto.Name:
                    results = await SearchByNameAsync(request.Query, request.MaxResults);
                    break;
                case DrugSearchTypeDto.NDC:
                    var drugByNdc = await GetDrugByNDCAsync(request.Query);
                    if (drugByNdc != null)
                    {
                        results.Add(MapToSummary(drugByNdc));
                    }
                    break;
                case DrugSearchTypeDto.RxCUI:
                    var drugByRxCui = await GetDrugByRxCUIAsync(request.Query);
                    if (drugByRxCui != null)
                    {
                        results.Add(MapToSummary(drugByRxCui));
                    }
                    break;
                case DrugSearchTypeDto.Ingredient:
                    results = await SearchByIngredientAsync(request.Query, request.MaxResults);
                    break;
                case DrugSearchTypeDto.BrandName:
                    results = await SearchByBrandNameAsync(request.Query, request.MaxResults);
                    break;
            }

            // Filter results
            if (!request.IncludeBrandNames)
            {
                results = results.Where(r => !r.IsBrand).ToList();
            }
            if (!request.IncludeGenerics)
            {
                results = results.Where(r => !r.IsGeneric).ToList();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching drugs with query: {Query}", request.Query);
        }

        return new DrugSearchResultDto
        {
            Results = results.Take(request.MaxResults).ToList(),
            TotalCount = results.Count,
            SearchTerm = request.Query,
            SearchType = request.SearchType
        };
    }

    public async Task<IEnumerable<DrugSummaryDto>> GetAutocompleteSuggestionsAsync(string query, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return Enumerable.Empty<DrugSummaryDto>();

        var cacheKey = $"{CacheKeyPrefix}Autocomplete_{query}";
        if (_cache.TryGetValue(cacheKey, out List<DrugSummaryDto>? cached))
        {
            return cached ?? Enumerable.Empty<DrugSummaryDto>();
        }

        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/spellingsuggestions.json?name={Uri.EscapeDataString(query)}");

            var suggestions = new List<DrugSummaryDto>();

            if (response.TryGetProperty("suggestionGroup", out var suggestionGroup) &&
                suggestionGroup.TryGetProperty("suggestionList", out var suggestionList) &&
                suggestionList.TryGetProperty("suggestion", out var suggestionArray))
            {
                foreach (var suggestion in suggestionArray.EnumerateArray().Take(maxResults))
                {
                    var name = suggestion.GetString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        suggestions.Add(new DrugSummaryDto { Name = name });
                    }
                }
            }

            _cache.Set(cacheKey, suggestions, TimeSpan.FromMinutes(30));
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autocomplete suggestions for: {Query}", query);
            return Enumerable.Empty<DrugSummaryDto>();
        }
    }

    public async Task<DrugDetailDto?> GetDrugByRxCUIAsync(string rxCUI)
    {
        var cacheKey = $"{CacheKeyPrefix}Detail_{rxCUI}";
        if (_cache.TryGetValue(cacheKey, out DrugDetailDto? cached))
        {
            return cached;
        }

        try
        {
            // Get basic drug info
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxcui/{rxCUI}/properties.json");

            if (!response.TryGetProperty("properties", out var properties))
                return null;

            var drug = new DrugDetailDto
            {
                RxCUI = rxCUI,
                Name = properties.TryGetProperty("name", out var name) ? name.GetString()! : "",
                Strength = properties.TryGetProperty("strength", out var strength) ? strength.GetString() : null,
                DoseForm = properties.TryGetProperty("doseFormConcept", out var df) ? df.GetString() : null,
                Route = properties.TryGetProperty("route", out var route) ? route.GetString() : null
            };

            // Get ingredients
            drug.Ingredients = await GetDrugIngredientsAsync(rxCUI);

            // Get NDCs
            drug.NDCs = await GetNDCsForDrugInternalAsync(rxCUI);

            // Get FDA info
            drug.FDAInfo = await GetFDAInfoAsync(rxCUI);

            _cache.Set(cacheKey, drug, CacheDuration);
            return drug;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug by RxCUI: {RxCUI}", rxCUI);
            return null;
        }
    }

    public async Task<DrugDetailDto?> GetDrugByNDCAsync(string ndc)
    {
        try
        {
            // Normalize NDC
            var normalizedNdc = NormalizeNDC(ndc);

            // Get RxCUI from NDC
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/ndcstatus.json?ndc={normalizedNdc}");

            if (response.TryGetProperty("ndcStatus", out var ndcStatus) &&
                ndcStatus.TryGetProperty("rxcui", out var rxcui))
            {
                var rxCUI = rxcui.GetString();
                if (!string.IsNullOrEmpty(rxCUI))
                {
                    return await GetDrugByRxCUIAsync(rxCUI);
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug by NDC: {NDC}", ndc);
            return null;
        }
    }

    public async Task<IEnumerable<DrugAlternativeDto>> GetDrugAlternativesAsync(string rxCUI)
    {
        var alternatives = new List<DrugAlternativeDto>();

        try
        {
            // Get generic alternatives
            var generics = await GetGenericEquivalentsAsync(rxCUI);
            foreach (var generic in generics)
            {
                alternatives.Add(new DrugAlternativeDto
                {
                    RxCUI = generic.RxCUI,
                    Name = generic.Name,
                    Strength = generic.Strength,
                    DoseForm = generic.DoseForm,
                    AlternativeType = AlternativeTypeDto.GenericEquivalent,
                    IsGeneric = true
                });
            }

            // Get therapeutic alternatives from RxClass
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxclass/class/byRxcui.json?rxcui={rxCUI}&relaSource=NDFRT");

            // Additional therapeutic alternatives logic would go here
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug alternatives for RxCUI: {RxCUI}", rxCUI);
        }

        return alternatives;
    }

    public async Task<IEnumerable<DrugSummaryDto>> GetBrandNamesAsync(string genericRxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxcui/{genericRxCUI}/related.json?tty=BN");

            return ParseRelatedDrugs(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand names for RxCUI: {RxCUI}", genericRxCUI);
            return Enumerable.Empty<DrugSummaryDto>();
        }
    }

    public async Task<IEnumerable<DrugSummaryDto>> GetGenericEquivalentsAsync(string brandRxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxcui/{brandRxCUI}/related.json?tty=SCD+SBD");

            return ParseRelatedDrugs(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting generic equivalents for RxCUI: {RxCUI}", brandRxCUI);
            return Enumerable.Empty<DrugSummaryDto>();
        }
    }

    #endregion

    #region Drug Interactions

    public async Task<DrugInteractionCheckResponseDto> CheckInteractionsAsync(DrugInteractionCheckRequestDto request)
    {
        var response = new DrugInteractionCheckResponseDto();

        if (request.RxCUIs.Count < 2)
        {
            return response;
        }

        try
        {
            var rxCUIList = string.Join("+", request.RxCUIs);
            var apiResponse = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/interaction/list.json?rxcuis={rxCUIList}");

            if (apiResponse.TryGetProperty("fullInteractionTypeGroup", out var interactionGroups))
            {
                foreach (var group in interactionGroups.EnumerateArray())
                {
                    if (group.TryGetProperty("fullInteractionType", out var interactions))
                    {
                        foreach (var interaction in interactions.EnumerateArray())
                        {
                            var interactionDto = ParseInteraction(interaction);
                            if (interactionDto != null)
                            {
                                response.Interactions.Add(interactionDto);

                                switch (interactionDto.Severity)
                                {
                                    case InteractionSeverityDto.Severe:
                                    case InteractionSeverityDto.Contraindicated:
                                        response.SevereInteractions++;
                                        break;
                                    case InteractionSeverityDto.Moderate:
                                        response.ModerateInteractions++;
                                        break;
                                    case InteractionSeverityDto.Minor:
                                        response.MinorInteractions++;
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            response.HasInteractions = response.Interactions.Any();
            response.TotalInteractions = response.Interactions.Count;

            // Check allergies if patient ID provided
            if (request.IncludeAllergies && request.PatientId.HasValue)
            {
                response.AllergyAlerts = new List<DrugAllergyAlertDto>();
                foreach (var rxCUI in request.RxCUIs)
                {
                    var allergyAlerts = await CheckAllergyInteractionsAsync(request.PatientId.Value, rxCUI);
                    response.AllergyAlerts.AddRange(allergyAlerts);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking drug interactions");
        }

        return response;
    }

    public async Task<DrugInteractionCheckResponseDto> CheckInteractionsByRxCUIsAsync(IEnumerable<string> rxCUIs)
    {
        return await CheckInteractionsAsync(new DrugInteractionCheckRequestDto
        {
            RxCUIs = rxCUIs.ToList()
        });
    }

    public async Task<IEnumerable<DrugAllergyAlertDto>> CheckAllergyInteractionsAsync(int patientId, string rxCUI)
    {
        var alerts = new List<DrugAllergyAlertDto>();

        try
        {
            // Get patient allergies
            var patientAllergies = await _context.PatientAllergies
                .Where(a => a.PatientId == patientId && a.IsActive)
                .ToListAsync();

            if (!patientAllergies.Any())
                return alerts;

            // Get drug ingredients
            var ingredients = await GetDrugIngredientsAsync(rxCUI);

            foreach (var allergy in patientAllergies)
            {
                // Check if any ingredient matches allergy
                var matchingIngredient = ingredients.FirstOrDefault(i =>
                    i.Name.Contains(allergy.AllergyName, StringComparison.OrdinalIgnoreCase));

                if (matchingIngredient != null)
                {
                    var drug = await GetDrugByRxCUIAsync(rxCUI);
                    alerts.Add(new DrugAllergyAlertDto
                    {
                        DrugRxCUI = rxCUI,
                        DrugName = drug?.Name ?? "",
                        AllergenName = allergy.AllergyName,
                        AllergyType = allergy.AllergyType,
                        RecommendedAction = "Do not prescribe - patient has documented allergy"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking allergy interactions for patient {PatientId}", patientId);
        }

        return alerts;
    }

    public async Task<IEnumerable<DrugInteractionDto>> GetKnownInteractionsAsync(string rxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/interaction/interaction.json?rxcui={rxCUI}");

            var interactions = new List<DrugInteractionDto>();

            if (response.TryGetProperty("interactionTypeGroup", out var groups))
            {
                foreach (var group in groups.EnumerateArray())
                {
                    if (group.TryGetProperty("interactionType", out var types))
                    {
                        foreach (var type in types.EnumerateArray())
                        {
                            var interaction = ParseInteraction(type);
                            if (interaction != null)
                            {
                                interactions.Add(interaction);
                            }
                        }
                    }
                }
            }

            return interactions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting known interactions for RxCUI: {RxCUI}", rxCUI);
            return Enumerable.Empty<DrugInteractionDto>();
        }
    }

    #endregion

    #region Drug Classes

    public async Task<IEnumerable<DrugClassDto>> GetDrugClassesAsync(string classType = "ATC")
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxclass/classTree.json?classId=ATC&relaSource=ATC");

            var classes = new List<DrugClassDto>();
            // Parse class hierarchy
            return classes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug classes for type: {ClassType}", classType);
            return Enumerable.Empty<DrugClassDto>();
        }
    }

    public async Task<IEnumerable<DrugSummaryDto>> GetDrugsByClassAsync(DrugsByClassRequestDto request)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxclass/classMembers.json?classId={request.ClassId}&relaSource={request.ClassType}");

            var drugs = new List<DrugSummaryDto>();

            if (response.TryGetProperty("drugMemberGroup", out var memberGroup) &&
                memberGroup.TryGetProperty("drugMember", out var members))
            {
                foreach (var member in members.EnumerateArray().Take(request.MaxResults))
                {
                    if (member.TryGetProperty("minConcept", out var concept))
                    {
                        drugs.Add(new DrugSummaryDto
                        {
                            RxCUI = concept.TryGetProperty("rxcui", out var rxcui) ? rxcui.GetString()! : "",
                            Name = concept.TryGetProperty("name", out var name) ? name.GetString()! : ""
                        });
                    }
                }
            }

            return drugs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drugs by class: {ClassId}", request.ClassId);
            return Enumerable.Empty<DrugSummaryDto>();
        }
    }

    public async Task<IEnumerable<DrugClassDto>> GetDrugClassHierarchyAsync(string rxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxclass/class/byRxcui.json?rxcui={rxCUI}");

            var classes = new List<DrugClassDto>();

            if (response.TryGetProperty("rxclassMinConceptList", out var conceptList) &&
                conceptList.TryGetProperty("rxclassMinConcept", out var concepts))
            {
                foreach (var concept in concepts.EnumerateArray())
                {
                    classes.Add(new DrugClassDto
                    {
                        ClassId = concept.TryGetProperty("classId", out var id) ? id.GetString()! : "",
                        ClassName = concept.TryGetProperty("className", out var name) ? name.GetString()! : "",
                        ClassType = concept.TryGetProperty("classType", out var type) ? type.GetString()! : ""
                    });
                }
            }

            return classes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug class hierarchy for RxCUI: {RxCUI}", rxCUI);
            return Enumerable.Empty<DrugClassDto>();
        }
    }

    #endregion

    #region Formulary

    public async Task<FormularyCheckResponseDto> CheckFormularyAsync(int branchId, string rxCUI)
    {
        var formularyDrug = await _context.FormularyDrugs
            .FirstOrDefaultAsync(f => f.BranchId == branchId &&
                                      f.RxCUI == rxCUI &&
                                      f.IsActive);

        var drug = await GetDrugByRxCUIAsync(rxCUI);

        if (formularyDrug != null)
        {
            return new FormularyCheckResponseDto
            {
                RxCUI = rxCUI,
                DrugName = drug?.Name ?? "",
                IsOnFormulary = true,
                FormularyInfo = MapToFormularyDto(formularyDrug)
            };
        }

        // Get alternatives that are on formulary
        var alternatives = await GetFormularyAlternativesAsync(branchId, rxCUI);

        return new FormularyCheckResponseDto
        {
            RxCUI = rxCUI,
            DrugName = drug?.Name ?? "",
            IsOnFormulary = false,
            Alternatives = alternatives.ToList(),
            Message = alternatives.Any()
                ? $"Drug not on formulary. {alternatives.Count()} alternatives available."
                : "Drug not on formulary. No alternatives found."
        };
    }

    public async Task<IEnumerable<FormularyDrugDto>> GetFormularyDrugsAsync(int branchId, FormularyTierDto? tier = null)
    {
        var query = _context.FormularyDrugs
            .Where(f => f.BranchId == branchId && f.IsActive);

        if (tier.HasValue)
        {
            query = query.Where(f => f.Tier == tier.Value.ToString());
        }

        var drugs = await query
            .OrderBy(f => f.Tier)
            .ThenBy(f => f.DrugName)
            .ToListAsync();

        return drugs.Select(MapToFormularyDto);
    }

    public async Task<FormularyDrugDto> AddToFormularyAsync(int branchId, AddToFormularyRequestDto request)
    {
        var drug = await GetDrugByRxCUIAsync(request.RxCUI);

        var formularyDrug = new FormularyDrug
        {
            BranchId = branchId,
            RxCUI = request.RxCUI,
            DrugName = drug?.Name ?? "",
            GenericName = drug?.GenericName,
            Strength = drug?.Strength,
            DoseForm = drug?.DoseForm,
            Tier = request.Tier.ToString(),
            RequiresPriorAuth = request.RequiresPriorAuth,
            HasQuantityLimit = request.HasQuantityLimit,
            QuantityLimit = request.QuantityLimit,
            QuantityLimitUnit = request.QuantityLimitUnit,
            DaysSupplyLimit = request.DaysSupplyLimit,
            HasStepTherapy = request.HasStepTherapy,
            StepTherapyRequirement = request.StepTherapyRequirement,
            Copay = request.Copay,
            CoinsurancePercent = request.CoinsurancePercent,
            IsPreferred = request.IsPreferred,
            Notes = request.Notes,
            IsActive = true,
            EffectiveDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.FormularyDrugs.Add(formularyDrug);
        await _context.SaveChangesAsync();

        return MapToFormularyDto(formularyDrug);
    }

    public async Task<FormularyDrugDto> UpdateFormularyDrugAsync(int formularyDrugId, AddToFormularyRequestDto request)
    {
        var formularyDrug = await _context.FormularyDrugs.FindAsync(formularyDrugId);
        if (formularyDrug == null)
            return new FormularyDrugDto();

        formularyDrug.Tier = request.Tier.ToString();
        formularyDrug.RequiresPriorAuth = request.RequiresPriorAuth;
        formularyDrug.HasQuantityLimit = request.HasQuantityLimit;
        formularyDrug.QuantityLimit = request.QuantityLimit;
        formularyDrug.QuantityLimitUnit = request.QuantityLimitUnit;
        formularyDrug.DaysSupplyLimit = request.DaysSupplyLimit;
        formularyDrug.HasStepTherapy = request.HasStepTherapy;
        formularyDrug.StepTherapyRequirement = request.StepTherapyRequirement;
        formularyDrug.Copay = request.Copay;
        formularyDrug.CoinsurancePercent = request.CoinsurancePercent;
        formularyDrug.IsPreferred = request.IsPreferred;
        formularyDrug.Notes = request.Notes;
        formularyDrug.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToFormularyDto(formularyDrug);
    }

    public async Task<bool> RemoveFromFormularyAsync(int formularyDrugId)
    {
        var formularyDrug = await _context.FormularyDrugs.FindAsync(formularyDrugId);
        if (formularyDrug == null)
            return false;

        formularyDrug.IsActive = false;
        formularyDrug.EndDate = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<FormularyDrugDto>> GetFormularyAlternativesAsync(int branchId, string rxCUI)
    {
        var alternatives = await GetDrugAlternativesAsync(rxCUI);
        var alternativeRxCUIs = alternatives.Select(a => a.RxCUI).ToList();

        var formularyAlternatives = await _context.FormularyDrugs
            .Where(f => f.BranchId == branchId &&
                        alternativeRxCUIs.Contains(f.RxCUI) &&
                        f.IsActive)
            .OrderBy(f => f.Tier)
            .ToListAsync();

        return formularyAlternatives.Select(MapToFormularyDto);
    }

    #endregion

    #region Drug Monograph

    public async Task<DrugMonographDto?> GetMonographAsync(string rxCUI)
    {
        try
        {
            // Get drug label from OpenFDA
            var drug = await GetDrugByRxCUIAsync(rxCUI);
            if (drug == null)
                return null;

            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{OpenFDAApiBase}/label.json?search=openfda.rxcui:\"{rxCUI}\"&limit=1");

            if (!response.TryGetProperty("results", out var results) ||
                results.GetArrayLength() == 0)
            {
                return null;
            }

            var label = results[0];

            return new DrugMonographDto
            {
                RxCUI = rxCUI,
                DrugName = drug.Name,
                GenericName = drug.GenericName,
                Description = GetMonographSection(label, "description"),
                Indications = GetMonographSection(label, "indications_and_usage"),
                DosageAndAdministration = GetMonographSection(label, "dosage_and_administration"),
                Contraindications = GetMonographSection(label, "contraindications"),
                WarningsAndPrecautions = GetMonographSection(label, "warnings_and_precautions"),
                AdverseReactions = GetMonographSection(label, "adverse_reactions"),
                DrugInteractions = GetMonographSection(label, "drug_interactions"),
                UseInSpecificPopulations = GetMonographSection(label, "use_in_specific_populations"),
                Overdosage = GetMonographSection(label, "overdosage"),
                ClinicalPharmacology = GetMonographSection(label, "clinical_pharmacology"),
                Storage = GetMonographSection(label, "storage_and_handling"),
                PatientCounseling = GetMonographSection(label, "information_for_patients"),
                Source = "OpenFDA"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting monograph for RxCUI: {RxCUI}", rxCUI);
            return null;
        }
    }

    public async Task<MonographSectionDto?> GetMonographSectionAsync(string rxCUI, string sectionName)
    {
        var monograph = await GetMonographAsync(rxCUI);
        if (monograph == null)
            return null;

        return sectionName.ToLower() switch
        {
            "description" => monograph.Description,
            "indications" => monograph.Indications,
            "dosage" => monograph.DosageAndAdministration,
            "contraindications" => monograph.Contraindications,
            "warnings" => monograph.WarningsAndPrecautions,
            "adverse" => monograph.AdverseReactions,
            "interactions" => monograph.DrugInteractions,
            _ => null
        };
    }

    #endregion

    #region Pricing

    public async Task<DrugPricingDto?> GetDrugPricingAsync(string rxCUI)
    {
        // Check local pricing database first
        var localPricing = await _context.DrugPricings
            .Where(p => p.RxCUI == rxCUI && p.IsActive)
            .OrderByDescending(p => p.PriceDate)
            .FirstOrDefaultAsync();

        if (localPricing != null)
        {
            return new DrugPricingDto
            {
                AWP = localPricing.AWP,
                WAC = localPricing.WAC,
                MAC = localPricing.MAC,
                NADAC = localPricing.NADAC,
                Unit = localPricing.Unit,
                PriceDate = localPricing.PriceDate,
                Source = localPricing.Source
            };
        }

        // Would integrate with pricing API here
        return null;
    }

    public async Task<DrugPricingDto?> GetDrugPricingByNDCAsync(string ndc)
    {
        var drug = await GetDrugByNDCAsync(ndc);
        if (drug == null)
            return null;

        return await GetDrugPricingAsync(drug.RxCUI);
    }

    public async Task<IEnumerable<(DrugSummaryDto Drug, DrugPricingDto? Pricing)>> ComparePricesAsync(IEnumerable<string> rxCUIs)
    {
        var results = new List<(DrugSummaryDto Drug, DrugPricingDto? Pricing)>();

        foreach (var rxCUI in rxCUIs)
        {
            var drug = await GetDrugByRxCUIAsync(rxCUI);
            var pricing = await GetDrugPricingAsync(rxCUI);

            if (drug != null)
            {
                results.Add((MapToSummary(drug), pricing));
            }
        }

        return results.OrderBy(r => r.Pricing?.AWP ?? decimal.MaxValue);
    }

    #endregion

    #region Patient Assistance

    public async Task<IEnumerable<PatientAssistanceProgramDto>> FindAssistanceProgramsAsync(FindAssistanceProgramsRequestDto request)
    {
        var query = _context.PatientAssistancePrograms.Where(p => p.IsActive);

        if (!string.IsNullOrEmpty(request.RxCUI))
        {
            query = query.Where(p => p.RxCUI == request.RxCUI);
        }

        if (!string.IsNullOrEmpty(request.DrugName))
        {
            query = query.Where(p => p.DrugName!.Contains(request.DrugName));
        }

        var programs = await query.ToListAsync();

        return programs.Select(p => new PatientAssistanceProgramDto
        {
            Id = p.Id,
            ProgramName = p.ProgramName,
            Sponsor = p.Sponsor,
            DrugName = p.DrugName,
            RxCUI = p.RxCUI,
            Description = p.Description,
            EligibilityRequirements = p.EligibilityRequirements,
            IncomeRequirement = p.IncomeRequirement,
            InsuranceRequirement = p.InsuranceRequirement,
            ApplicationUrl = p.ApplicationUrl,
            PhoneNumber = p.PhoneNumber,
            MaxSavings = p.MaxSavings,
            CouponCode = p.CouponCode,
            ExpirationDate = p.ExpirationDate,
            IsActive = p.IsActive
        });
    }

    public async Task<IEnumerable<PatientAssistanceProgramDto>> GetCopayCardsAsync(string rxCUI)
    {
        return await FindAssistanceProgramsAsync(new FindAssistanceProgramsRequestDto
        {
            RxCUI = rxCUI
        });
    }

    #endregion

    #region Controlled Substances

    public async Task<ControlledSubstanceInfoDto?> GetControlledSubstanceInfoAsync(string rxCUI)
    {
        var info = await _context.ControlledSubstanceInfos
            .FirstOrDefaultAsync(c => c.RxCUI == rxCUI);

        if (info == null)
            return null;

        return new ControlledSubstanceInfoDto
        {
            RxCUI = info.RxCUI,
            DrugName = info.DrugName,
            DEASchedule = info.DEASchedule,
            StateSchedule = info.StateSchedule,
            RequiresEPCS = info.RequiresEPCS,
            MaxDaysSupply = info.MaxDaysSupply,
            RefillsAllowed = info.RefillsAllowed,
            MaxRefills = info.MaxRefills,
            SpecialRequirements = info.SpecialRequirements,
            RequiresPDMP = info.RequiresPDMP
        };
    }

    public async Task<bool> IsControlledSubstanceAsync(string rxCUI)
    {
        return await _context.ControlledSubstanceInfos
            .AnyAsync(c => c.RxCUI == rxCUI);
    }

    public async Task<string?> GetDEAScheduleAsync(string rxCUI)
    {
        var info = await GetControlledSubstanceInfoAsync(rxCUI);
        return info?.DEASchedule;
    }

    #endregion

    #region FDA Information

    public async Task<DrugFDAInfoDto?> GetFDAInfoAsync(string rxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{OpenFDAApiBase}/label.json?search=openfda.rxcui:\"{rxCUI}\"&limit=1");

            if (!response.TryGetProperty("results", out var results) ||
                results.GetArrayLength() == 0)
            {
                return null;
            }

            var label = results[0];
            var openfda = label.TryGetProperty("openfda", out var o) ? o : default;

            return new DrugFDAInfoDto
            {
                ApplicationNumber = openfda.TryGetProperty("application_number", out var an)
                    ? an[0].GetString()
                    : null,
                Sponsor = openfda.TryGetProperty("manufacturer_name", out var mn)
                    ? mn[0].GetString()
                    : null,
                HasBoxedWarning = label.TryGetProperty("boxed_warning", out _),
                BoxedWarningText = label.TryGetProperty("boxed_warning", out var bw)
                    ? string.Join("\n", bw.EnumerateArray().Select(b => b.GetString()))
                    : null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting FDA info for RxCUI: {RxCUI}", rxCUI);
            return null;
        }
    }

    public async Task<IEnumerable<object>> GetDrugRecallsAsync(string? rxCUI = null, DateTime? fromDate = null)
    {
        try
        {
            var search = "status:ongoing";
            if (!string.IsNullOrEmpty(rxCUI))
            {
                search += $"+AND+openfda.rxcui:\"{rxCUI}\"";
            }

            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{OpenFDAApiBase}/enforcement.json?search={search}&limit=100");

            // Parse and return recalls
            return new List<object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting drug recalls");
            return Enumerable.Empty<object>();
        }
    }

    public async Task<AdverseEventResponseDto> ReportAdverseEventAsync(int branchId, ReportAdverseEventDto report)
    {
        var adverseEvent = new DrugAdverseEvent
        {
            BranchId = branchId,
            DrugRxCUI = report.DrugRxCUI,
            DrugName = report.DrugName,
            PatientId = report.PatientId,
            PatientAge = report.PatientAge,
            PatientGender = report.PatientGender,
            EventDescription = report.EventDescription,
            Outcome = report.Outcome,
            OnsetDate = report.OnsetDate,
            StopDate = report.StopDate,
            DoseAtEvent = report.DoseAtEvent,
            RouteOfAdmin = report.RouteOfAdmin,
            Indication = report.Indication,
            ConcomitantMedications = report.ConcomitantMedications != null
                ? string.Join(", ", report.ConcomitantMedications)
                : null,
            ReporterType = report.ReporterType,
            ReporterName = report.ReporterName,
            ReporterContact = report.ReporterContact,
            ReportedAt = DateTime.UtcNow,
            Status = "Pending"
        };

        _context.DrugAdverseEvents.Add(adverseEvent);
        await _context.SaveChangesAsync();

        return new AdverseEventResponseDto
        {
            Success = true,
            ReportId = adverseEvent.Id.ToString(),
            Message = "Adverse event reported successfully"
        };
    }

    #endregion

    #region NDC Operations

    public async Task<DrugNDCDto?> ConvertNDCAsync(string ndc)
    {
        var normalizedNdc = NormalizeNDC(ndc);

        return new DrugNDCDto
        {
            NDC = normalizedNdc,
            NDC10 = FormatNDC10(normalizedNdc),
            NDC11 = FormatNDC11(normalizedNdc)
        };
    }

    public async Task<IEnumerable<DrugNDCDto>> GetNDCsForDrugAsync(string rxCUI)
    {
        return await GetNDCsForDrugInternalAsync(rxCUI);
    }

    public async Task<(bool IsValid, string? Message)> ValidateNDCAsync(string ndc)
    {
        var normalized = NormalizeNDC(ndc);
        if (normalized.Length != 11)
        {
            return (false, "Invalid NDC format");
        }

        // Check if NDC exists
        var drug = await GetDrugByNDCAsync(ndc);
        if (drug == null)
        {
            return (false, "NDC not found in database");
        }

        return (true, null);
    }

    #endregion

    #region Caching & Updates

    public async Task RefreshDrugCacheAsync()
    {
        // Clear all drug-related cache entries
        _logger.LogInformation("Refreshing drug cache");
    }

    public async Task<DateTime?> GetLastUpdateTimeAsync()
    {
        return await _context.DrugDatabaseUpdates
            .OrderByDescending(u => u.UpdatedAt)
            .Select(u => (DateTime?)u.UpdatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SyncRxNormUpdatesAsync()
    {
        // Check for RxNorm updates and sync
        _logger.LogInformation("Syncing RxNorm updates");
        return 0;
    }

    #endregion

    #region Private Helper Methods

    private async Task<List<DrugSummaryDto>> SearchByNameAsync(string query, int maxResults)
    {
        var response = await _httpClient.GetFromJsonAsync<JsonElement>(
            $"{RxNormApiBase}/drugs.json?name={Uri.EscapeDataString(query)}");

        return ParseDrugGroup(response, maxResults);
    }

    private async Task<List<DrugSummaryDto>> SearchByIngredientAsync(string ingredient, int maxResults)
    {
        var response = await _httpClient.GetFromJsonAsync<JsonElement>(
            $"{RxNormApiBase}/drugs.json?ingredient={Uri.EscapeDataString(ingredient)}");

        return ParseDrugGroup(response, maxResults);
    }

    private async Task<List<DrugSummaryDto>> SearchByBrandNameAsync(string brandName, int maxResults)
    {
        var response = await _httpClient.GetFromJsonAsync<JsonElement>(
            $"{RxNormApiBase}/drugs.json?name={Uri.EscapeDataString(brandName)}");

        var results = ParseDrugGroup(response, maxResults);
        return results.Where(r => r.IsBrand).ToList();
    }

    private static List<DrugSummaryDto> ParseDrugGroup(JsonElement response, int maxResults)
    {
        var drugs = new List<DrugSummaryDto>();

        if (!response.TryGetProperty("drugGroup", out var drugGroup))
            return drugs;

        if (!drugGroup.TryGetProperty("conceptGroup", out var conceptGroups))
            return drugs;

        foreach (var group in conceptGroups.EnumerateArray())
        {
            if (!group.TryGetProperty("conceptProperties", out var concepts))
                continue;

            foreach (var concept in concepts.EnumerateArray())
            {
                if (drugs.Count >= maxResults)
                    return drugs;

                drugs.Add(new DrugSummaryDto
                {
                    RxCUI = concept.TryGetProperty("rxcui", out var rxcui) ? rxcui.GetString()! : "",
                    Name = concept.TryGetProperty("name", out var name) ? name.GetString()! : "",
                    TermType = ParseTermType(group.TryGetProperty("tty", out var tty) ? tty.GetString() : null)
                });
            }
        }

        return drugs;
    }

    private static List<DrugSummaryDto> ParseRelatedDrugs(JsonElement response)
    {
        var drugs = new List<DrugSummaryDto>();

        if (!response.TryGetProperty("relatedGroup", out var relatedGroup))
            return drugs;

        if (!relatedGroup.TryGetProperty("conceptGroup", out var conceptGroups))
            return drugs;

        foreach (var group in conceptGroups.EnumerateArray())
        {
            if (!group.TryGetProperty("conceptProperties", out var concepts))
                continue;

            foreach (var concept in concepts.EnumerateArray())
            {
                drugs.Add(new DrugSummaryDto
                {
                    RxCUI = concept.TryGetProperty("rxcui", out var rxcui) ? rxcui.GetString()! : "",
                    Name = concept.TryGetProperty("name", out var name) ? name.GetString()! : ""
                });
            }
        }

        return drugs;
    }

    private async Task<List<DrugIngredientDto>> GetDrugIngredientsAsync(string rxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxcui/{rxCUI}/related.json?tty=IN+MIN");

            var ingredients = new List<DrugIngredientDto>();

            if (response.TryGetProperty("relatedGroup", out var relatedGroup) &&
                relatedGroup.TryGetProperty("conceptGroup", out var groups))
            {
                foreach (var group in groups.EnumerateArray())
                {
                    if (group.TryGetProperty("conceptProperties", out var concepts))
                    {
                        foreach (var concept in concepts.EnumerateArray())
                        {
                            ingredients.Add(new DrugIngredientDto
                            {
                                RxCUI = concept.TryGetProperty("rxcui", out var rxcui) ? rxcui.GetString()! : "",
                                Name = concept.TryGetProperty("name", out var name) ? name.GetString()! : ""
                            });
                        }
                    }
                }
            }

            return ingredients;
        }
        catch
        {
            return new List<DrugIngredientDto>();
        }
    }

    private async Task<List<DrugNDCDto>> GetNDCsForDrugInternalAsync(string rxCUI)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(
                $"{RxNormApiBase}/rxcui/{rxCUI}/ndcs.json");

            var ndcs = new List<DrugNDCDto>();

            if (response.TryGetProperty("ndcGroup", out var ndcGroup) &&
                ndcGroup.TryGetProperty("ndcList", out var ndcList) &&
                ndcList.TryGetProperty("ndc", out var ndcArray))
            {
                foreach (var ndc in ndcArray.EnumerateArray())
                {
                    var ndcValue = ndc.GetString();
                    if (!string.IsNullOrEmpty(ndcValue))
                    {
                        ndcs.Add(new DrugNDCDto { NDC = ndcValue });
                    }
                }
            }

            return ndcs;
        }
        catch
        {
            return new List<DrugNDCDto>();
        }
    }

    private static DrugInteractionDto? ParseInteraction(JsonElement interaction)
    {
        if (!interaction.TryGetProperty("interactionPair", out var pairs))
            return null;

        foreach (var pair in pairs.EnumerateArray())
        {
            var interactionConcept = pair.TryGetProperty("interactionConcept", out var concepts)
                ? concepts
                : default;

            var drug1 = interactionConcept.GetArrayLength() > 0
                ? interactionConcept[0]
                : default;
            var drug2 = interactionConcept.GetArrayLength() > 1
                ? interactionConcept[1]
                : default;

            return new DrugInteractionDto
            {
                Drug1RxCUI = drug1.TryGetProperty("minConceptItem", out var min1) &&
                             min1.TryGetProperty("rxcui", out var rxcui1)
                    ? rxcui1.GetString()!
                    : "",
                Drug1Name = drug1.TryGetProperty("minConceptItem", out var minItem1) &&
                            minItem1.TryGetProperty("name", out var name1)
                    ? name1.GetString()!
                    : "",
                Drug2RxCUI = drug2.TryGetProperty("minConceptItem", out var min2) &&
                             min2.TryGetProperty("rxcui", out var rxcui2)
                    ? rxcui2.GetString()!
                    : "",
                Drug2Name = drug2.TryGetProperty("minConceptItem", out var minItem2) &&
                            minItem2.TryGetProperty("name", out var name2)
                    ? name2.GetString()!
                    : "",
                Description = pair.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                Severity = ParseSeverity(pair.TryGetProperty("severity", out var sev) ? sev.GetString() : null)
            };
        }

        return null;
    }

    private static MonographSectionDto? GetMonographSection(JsonElement label, string sectionName)
    {
        if (label.TryGetProperty(sectionName, out var section) && section.GetArrayLength() > 0)
        {
            return new MonographSectionDto
            {
                Title = sectionName.Replace("_", " ").ToUpper(),
                Content = string.Join("\n\n", section.EnumerateArray().Select(s => s.GetString()))
            };
        }
        return null;
    }

    private static DrugTermTypeDto ParseTermType(string? tty)
    {
        return tty switch
        {
            "IN" => DrugTermTypeDto.Ingredient,
            "BN" => DrugTermTypeDto.BrandName,
            "SCD" => DrugTermTypeDto.ClinicalDrug,
            "SBD" => DrugTermTypeDto.ClinicalDrug,
            "BPCK" => DrugTermTypeDto.BrandedPack,
            "GPCK" => DrugTermTypeDto.GenericPack,
            _ => DrugTermTypeDto.Other
        };
    }

    private static InteractionSeverityDto ParseSeverity(string? severity)
    {
        return severity?.ToLower() switch
        {
            "high" or "severe" => InteractionSeverityDto.Severe,
            "moderate" => InteractionSeverityDto.Moderate,
            "low" or "minor" => InteractionSeverityDto.Minor,
            "contraindicated" => InteractionSeverityDto.Contraindicated,
            _ => InteractionSeverityDto.Unknown
        };
    }

    private static string NormalizeNDC(string ndc)
    {
        return ndc.Replace("-", "").Replace(" ", "").PadLeft(11, '0');
    }

    private static string FormatNDC10(string ndc11)
    {
        // 5-4-1 or 5-3-2 or 4-4-2 format
        return $"{ndc11[..5]}-{ndc11.Substring(5, 4)}-{ndc11.Substring(9, 2)}";
    }

    private static string FormatNDC11(string ndc)
    {
        return ndc.PadLeft(11, '0');
    }

    private static DrugSummaryDto MapToSummary(DrugDetailDto detail)
    {
        return new DrugSummaryDto
        {
            RxCUI = detail.RxCUI,
            Name = detail.Name,
            BrandName = detail.BrandName,
            GenericName = detail.GenericName,
            Strength = detail.Strength,
            DoseForm = detail.DoseForm,
            Route = detail.Route
        };
    }

    private static FormularyDrugDto MapToFormularyDto(FormularyDrug drug)
    {
        return new FormularyDrugDto
        {
            Id = drug.Id,
            RxCUI = drug.RxCUI,
            DrugName = drug.DrugName,
            GenericName = drug.GenericName,
            Strength = drug.Strength,
            DoseForm = drug.DoseForm,
            Tier = Enum.TryParse<FormularyTierDto>(drug.Tier, out var tier) ? tier : FormularyTierDto.NonFormulary,
            RequiresPriorAuth = drug.RequiresPriorAuth,
            HasQuantityLimit = drug.HasQuantityLimit,
            QuantityLimit = drug.QuantityLimit,
            QuantityLimitUnit = drug.QuantityLimitUnit,
            DaysSupplyLimit = drug.DaysSupplyLimit,
            HasStepTherapy = drug.HasStepTherapy,
            StepTherapyRequirement = drug.StepTherapyRequirement,
            Copay = drug.Copay,
            CoinsurancePercent = drug.CoinsurancePercent,
            IsPreferred = drug.IsPreferred,
            EffectiveDate = drug.EffectiveDate,
            EndDate = drug.EndDate,
            Notes = drug.Notes
        };
    }

    #endregion
}

#region Drug Database Entities

public class FormularyDrug
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string? GenericName { get; set; }
    public string? Strength { get; set; }
    public string? DoseForm { get; set; }
    public string Tier { get; set; } = string.Empty;
    public bool RequiresPriorAuth { get; set; }
    public bool HasQuantityLimit { get; set; }
    public int? QuantityLimit { get; set; }
    public string? QuantityLimitUnit { get; set; }
    public int? DaysSupplyLimit { get; set; }
    public bool HasStepTherapy { get; set; }
    public string? StepTherapyRequirement { get; set; }
    public decimal? Copay { get; set; }
    public decimal? CoinsurancePercent { get; set; }
    public bool IsPreferred { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EffectiveDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DrugPricing
{
    public int Id { get; set; }
    public string RxCUI { get; set; } = string.Empty;
    public string? NDC { get; set; }
    public decimal? AWP { get; set; }
    public decimal? WAC { get; set; }
    public decimal? MAC { get; set; }
    public decimal? NADAC { get; set; }
    public string? Unit { get; set; }
    public DateTime PriceDate { get; set; }
    public string? Source { get; set; }
    public bool IsActive { get; set; }
}

public class PatientAssistanceProgram
{
    public int Id { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? Sponsor { get; set; }
    public string? DrugName { get; set; }
    public string? RxCUI { get; set; }
    public string? Description { get; set; }
    public string? EligibilityRequirements { get; set; }
    public string? IncomeRequirement { get; set; }
    public string? InsuranceRequirement { get; set; }
    public string? ApplicationUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public decimal? MaxSavings { get; set; }
    public string? CouponCode { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsActive { get; set; }
}

public class ControlledSubstanceInfo
{
    public int Id { get; set; }
    public string RxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public string DEASchedule { get; set; } = string.Empty;
    public string? StateSchedule { get; set; }
    public bool RequiresEPCS { get; set; }
    public int? MaxDaysSupply { get; set; }
    public bool? RefillsAllowed { get; set; }
    public int? MaxRefills { get; set; }
    public string? SpecialRequirements { get; set; }
    public bool RequiresPDMP { get; set; }
}

public class DrugAdverseEvent
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string DrugRxCUI { get; set; } = string.Empty;
    public string DrugName { get; set; } = string.Empty;
    public int? PatientId { get; set; }
    public string? PatientAge { get; set; }
    public string? PatientGender { get; set; }
    public string EventDescription { get; set; } = string.Empty;
    public string? Outcome { get; set; }
    public DateTime? OnsetDate { get; set; }
    public DateTime? StopDate { get; set; }
    public string? DoseAtEvent { get; set; }
    public string? RouteOfAdmin { get; set; }
    public string? Indication { get; set; }
    public string? ConcomitantMedications { get; set; }
    public string? ReporterType { get; set; }
    public string? ReporterName { get; set; }
    public string? ReporterContact { get; set; }
    public DateTime ReportedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FDAReportNumber { get; set; }
}

public class DrugDatabaseUpdate
{
    public int Id { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    public int RecordsAdded { get; set; }
    public int RecordsUpdated { get; set; }
    public string? Notes { get; set; }
}

#endregion

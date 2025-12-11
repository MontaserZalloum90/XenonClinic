using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Enums;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service implementation for Insurance operations
/// </summary>
public class InsuranceService : IInsuranceService
{
    private readonly ClinicDbContext _context;
    private readonly ISequenceGenerator _sequenceGenerator;

    public InsuranceService(ClinicDbContext context, ISequenceGenerator sequenceGenerator)
    {
        _context = context;
        _sequenceGenerator = sequenceGenerator;
    }

    #region Insurance Providers

    public async Task<IEnumerable<InsuranceProviderDto>> GetProvidersAsync(int branchId, bool activeOnly = false)
    {
        var query = _context.Set<InsuranceProvider>()
            .AsNoTracking()
            .Include(p => p.Plans)
            .Where(p => p.BranchId == branchId);

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var providers = await query.OrderBy(p => p.Name).ToListAsync();

        return providers.Select(p => new InsuranceProviderDto
        {
            Id = p.Id,
            BranchId = p.BranchId,
            ProviderCode = p.ProviderCode,
            Name = p.Name,
            NameAr = p.NameAr,
            ShortName = p.ShortName,
            InsuranceType = p.InsuranceType,
            IsActive = p.IsActive,
            ContactPerson = p.ContactPerson,
            Email = p.Email,
            Phone = p.Phone,
            Website = p.Website,
            Address = p.Address,
            City = p.City,
            Country = p.Country,
            PaymentTermsDays = p.PaymentTermsDays,
            DefaultDiscountPercent = p.DefaultDiscountPercent,
            PlansCount = p.Plans.Count,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });
    }

    public async Task<InsuranceProviderDto?> GetProviderByIdAsync(int id)
    {
        var provider = await _context.Set<InsuranceProvider>()
            .AsNoTracking()
            .Include(p => p.Plans)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (provider == null) return null;

        return MapToProviderDto(provider);
    }

    public async Task<InsuranceProviderDto?> GetProviderByCodeAsync(int branchId, string providerCode)
    {
        var provider = await _context.Set<InsuranceProvider>()
            .AsNoTracking()
            .Include(p => p.Plans)
            .FirstOrDefaultAsync(p => p.BranchId == branchId && p.ProviderCode == providerCode);

        if (provider == null) return null;

        return MapToProviderDto(provider);
    }

    public async Task<InsuranceProviderDto> CreateProviderAsync(int branchId, CreateInsuranceProviderDto dto)
    {
        var provider = new InsuranceProvider
        {
            BranchId = branchId,
            ProviderCode = dto.ProviderCode,
            Name = dto.Name,
            NameAr = dto.NameAr,
            ShortName = dto.ShortName,
            InsuranceType = dto.InsuranceType,
            IsActive = true,
            ContactPerson = dto.ContactPerson,
            Email = dto.Email,
            Phone = dto.Phone,
            Fax = dto.Fax,
            Website = dto.Website,
            Address = dto.Address,
            City = dto.City,
            Country = dto.Country,
            TaxNumber = dto.TaxNumber,
            LicenseNumber = dto.LicenseNumber,
            ClaimsEndpoint = dto.ClaimsEndpoint,
            EligibilityEndpoint = dto.EligibilityEndpoint,
            PaymentTermsDays = dto.PaymentTermsDays,
            DefaultDiscountPercent = dto.DefaultDiscountPercent,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<InsuranceProvider>().Add(provider);
        await _context.SaveChangesAsync();

        return MapToProviderDto(provider);
    }

    public async Task<InsuranceProviderDto> UpdateProviderAsync(int id, CreateInsuranceProviderDto dto)
    {
        var provider = await _context.Set<InsuranceProvider>().FindAsync(id);
        if (provider == null)
        {
            throw new KeyNotFoundException($"Insurance provider with ID {id} not found");
        }

        provider.ProviderCode = dto.ProviderCode;
        provider.Name = dto.Name;
        provider.NameAr = dto.NameAr;
        provider.ShortName = dto.ShortName;
        provider.InsuranceType = dto.InsuranceType;
        provider.ContactPerson = dto.ContactPerson;
        provider.Email = dto.Email;
        provider.Phone = dto.Phone;
        provider.Fax = dto.Fax;
        provider.Website = dto.Website;
        provider.Address = dto.Address;
        provider.City = dto.City;
        provider.Country = dto.Country;
        provider.TaxNumber = dto.TaxNumber;
        provider.LicenseNumber = dto.LicenseNumber;
        provider.ClaimsEndpoint = dto.ClaimsEndpoint;
        provider.EligibilityEndpoint = dto.EligibilityEndpoint;
        provider.PaymentTermsDays = dto.PaymentTermsDays;
        provider.DefaultDiscountPercent = dto.DefaultDiscountPercent;
        provider.Notes = dto.Notes;
        provider.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToProviderDto(provider);
    }

    public async Task DeleteProviderAsync(int id)
    {
        var provider = await _context.Set<InsuranceProvider>().FindAsync(id);
        if (provider != null)
        {
            _context.Set<InsuranceProvider>().Remove(provider);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Insurance Plans

    public async Task<IEnumerable<InsurancePlanDto>> GetPlansByProviderAsync(int providerId, bool activeOnly = false)
    {
        var query = _context.Set<InsurancePlan>()
            .AsNoTracking()
            .Include(p => p.Provider)
            .Where(p => p.ProviderId == providerId);

        if (activeOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var plans = await query.OrderBy(p => p.Name).ToListAsync();

        return plans.Select(MapToPlanDto);
    }

    public async Task<InsurancePlanDto?> GetPlanByIdAsync(int id)
    {
        var plan = await _context.Set<InsurancePlan>()
            .AsNoTracking()
            .Include(p => p.Provider)
            .FirstOrDefaultAsync(p => p.Id == id);

        return plan != null ? MapToPlanDto(plan) : null;
    }

    public async Task<InsurancePlanDto> CreatePlanAsync(int branchId, CreateInsurancePlanDto dto)
    {
        var plan = new InsurancePlan
        {
            BranchId = branchId,
            ProviderId = dto.ProviderId,
            PlanCode = dto.PlanCode,
            Name = dto.Name,
            NameAr = dto.NameAr,
            Description = dto.Description,
            PlanType = dto.PlanType,
            NetworkType = dto.NetworkType,
            IsActive = true,
            CoveragePercent = dto.CoveragePercent,
            CopayPercent = dto.CopayPercent,
            CopayAmount = dto.CopayAmount,
            DeductibleAmount = dto.DeductibleAmount,
            AnnualMaximum = dto.AnnualMaximum,
            LifetimeMaximum = dto.LifetimeMaximum,
            MaxPerVisit = dto.MaxPerVisit,
            RequiresPreAuth = dto.RequiresPreAuth,
            RequiresReferral = dto.RequiresReferral,
            CoveredServices = dto.CoveredServices,
            ExcludedServices = dto.ExcludedServices,
            WaitingPeriodDays = dto.WaitingPeriodDays,
            ClaimGracePeriodDays = dto.ClaimGracePeriodDays,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<InsurancePlan>().Add(plan);
        await _context.SaveChangesAsync();

        return MapToPlanDto(plan);
    }

    public async Task<InsurancePlanDto> UpdatePlanAsync(int id, CreateInsurancePlanDto dto)
    {
        var plan = await _context.Set<InsurancePlan>().FindAsync(id);
        if (plan == null)
        {
            throw new KeyNotFoundException($"Insurance plan with ID {id} not found");
        }

        plan.PlanCode = dto.PlanCode;
        plan.Name = dto.Name;
        plan.NameAr = dto.NameAr;
        plan.Description = dto.Description;
        plan.PlanType = dto.PlanType;
        plan.NetworkType = dto.NetworkType;
        plan.CoveragePercent = dto.CoveragePercent;
        plan.CopayPercent = dto.CopayPercent;
        plan.CopayAmount = dto.CopayAmount;
        plan.DeductibleAmount = dto.DeductibleAmount;
        plan.AnnualMaximum = dto.AnnualMaximum;
        plan.LifetimeMaximum = dto.LifetimeMaximum;
        plan.MaxPerVisit = dto.MaxPerVisit;
        plan.RequiresPreAuth = dto.RequiresPreAuth;
        plan.RequiresReferral = dto.RequiresReferral;
        plan.CoveredServices = dto.CoveredServices;
        plan.ExcludedServices = dto.ExcludedServices;
        plan.WaitingPeriodDays = dto.WaitingPeriodDays;
        plan.ClaimGracePeriodDays = dto.ClaimGracePeriodDays;
        plan.Notes = dto.Notes;
        plan.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToPlanDto(plan);
    }

    public async Task DeletePlanAsync(int id)
    {
        var plan = await _context.Set<InsurancePlan>().FindAsync(id);
        if (plan != null)
        {
            _context.Set<InsurancePlan>().Remove(plan);
            await _context.SaveChangesAsync();
        }
    }

    #endregion

    #region Patient Insurance

    public async Task<IEnumerable<PatientInsuranceDto>> GetPatientInsurancesAsync(int patientId)
    {
        var insurances = await _context.Set<PatientInsurance>()
            .AsNoTracking()
            .Include(pi => pi.Provider)
            .Include(pi => pi.Plan)
            .Include(pi => pi.Patient)
            .Where(pi => pi.PatientId == patientId)
            .OrderByDescending(pi => pi.IsPrimary)
            .ThenByDescending(pi => pi.EffectiveDate)
            .ToListAsync();

        return insurances.Select(MapToPatientInsuranceDto);
    }

    public async Task<PatientInsuranceDto?> GetPatientInsuranceByIdAsync(int id)
    {
        var insurance = await _context.Set<PatientInsurance>()
            .AsNoTracking()
            .Include(pi => pi.Provider)
            .Include(pi => pi.Plan)
            .Include(pi => pi.Patient)
            .FirstOrDefaultAsync(pi => pi.Id == id);

        return insurance != null ? MapToPatientInsuranceDto(insurance) : null;
    }

    public async Task<PatientInsuranceDto?> GetPrimaryInsuranceAsync(int patientId)
    {
        var insurance = await _context.Set<PatientInsurance>()
            .AsNoTracking()
            .Include(pi => pi.Provider)
            .Include(pi => pi.Plan)
            .Include(pi => pi.Patient)
            .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive);

        return insurance != null ? MapToPatientInsuranceDto(insurance) : null;
    }

    public async Task<PatientInsuranceDto> CreatePatientInsuranceAsync(int branchId, CreatePatientInsuranceDto dto)
    {
        // If this is primary, unset other primary insurances
        if (dto.IsPrimary)
        {
            var existingPrimary = await _context.Set<PatientInsurance>()
                .Where(pi => pi.PatientId == dto.PatientId && pi.IsPrimary)
                .ToListAsync();

            foreach (var pi in existingPrimary)
            {
                pi.IsPrimary = false;
            }
        }

        var insurance = new PatientInsurance
        {
            BranchId = branchId,
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,
            PlanId = dto.PlanId,
            MemberId = dto.MemberId,
            GroupNumber = dto.GroupNumber,
            SubscriberId = dto.SubscriberId,
            IsPrimary = dto.IsPrimary,
            RelationshipToSubscriber = dto.RelationshipToSubscriber,
            SubscriberName = dto.SubscriberName,
            SubscriberDateOfBirth = dto.SubscriberDateOfBirth,
            EffectiveDate = dto.EffectiveDate,
            TerminationDate = dto.TerminationDate,
            IsActive = true,
            CardImageFront = dto.CardImageFront,
            CardImageBack = dto.CardImageBack,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<PatientInsurance>().Add(insurance);
        await _context.SaveChangesAsync();

        return MapToPatientInsuranceDto(insurance);
    }

    public async Task<PatientInsuranceDto> UpdatePatientInsuranceAsync(int id, CreatePatientInsuranceDto dto)
    {
        var insurance = await _context.Set<PatientInsurance>().FindAsync(id);
        if (insurance == null)
        {
            throw new KeyNotFoundException($"Patient insurance with ID {id} not found");
        }

        // If this is becoming primary, unset other primary insurances
        if (dto.IsPrimary && !insurance.IsPrimary)
        {
            var existingPrimary = await _context.Set<PatientInsurance>()
                .Where(pi => pi.PatientId == dto.PatientId && pi.IsPrimary && pi.Id != id)
                .ToListAsync();

            foreach (var pi in existingPrimary)
            {
                pi.IsPrimary = false;
            }
        }

        insurance.ProviderId = dto.ProviderId;
        insurance.PlanId = dto.PlanId;
        insurance.MemberId = dto.MemberId;
        insurance.GroupNumber = dto.GroupNumber;
        insurance.SubscriberId = dto.SubscriberId;
        insurance.IsPrimary = dto.IsPrimary;
        insurance.RelationshipToSubscriber = dto.RelationshipToSubscriber;
        insurance.SubscriberName = dto.SubscriberName;
        insurance.SubscriberDateOfBirth = dto.SubscriberDateOfBirth;
        insurance.EffectiveDate = dto.EffectiveDate;
        insurance.TerminationDate = dto.TerminationDate;
        insurance.CardImageFront = dto.CardImageFront;
        insurance.CardImageBack = dto.CardImageBack;
        insurance.Notes = dto.Notes;
        insurance.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToPatientInsuranceDto(insurance);
    }

    public async Task TerminatePatientInsuranceAsync(int id, DateTime terminationDate)
    {
        var insurance = await _context.Set<PatientInsurance>().FindAsync(id);
        if (insurance != null)
        {
            insurance.TerminationDate = terminationDate;
            insurance.IsActive = false;
            insurance.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EligibilityVerificationResponseDto> VerifyEligibilityAsync(EligibilityVerificationRequestDto request)
    {
        var patientInsurance = await _context.Set<PatientInsurance>()
            .Include(pi => pi.Provider)
            .Include(pi => pi.Plan)
            .FirstOrDefaultAsync(pi => pi.Id == request.PatientInsuranceId);

        if (patientInsurance == null)
        {
            return new EligibilityVerificationResponseDto
            {
                IsEligible = false,
                Status = "NotFound",
                StatusMessage = "Patient insurance not found"
            };
        }

        // In production, this would call the insurance provider's eligibility API
        // For now, we return a simulated response
        var response = new EligibilityVerificationResponseDto
        {
            IsEligible = patientInsurance.IsActive &&
                        (patientInsurance.TerminationDate == null ||
                         patientInsurance.TerminationDate > DateTime.UtcNow),
            VerificationDate = DateTime.UtcNow,
            VerificationId = Guid.NewGuid().ToString("N")[..12].ToUpper(),
            Status = "Active",
            StatusMessage = "Coverage is active",
            CoverageStartDate = patientInsurance.EffectiveDate,
            CoverageEndDate = patientInsurance.TerminationDate,
            RemainingDeductible = patientInsurance.RemainingDeductible,
            RemainingAnnualBenefit = patientInsurance.RemainingAnnualBenefit,
            Copay = patientInsurance.Plan?.CopayAmount,
            CoinsurancePercent = patientInsurance.Plan?.CopayPercent
        };

        // Update last verified date
        patientInsurance.LastVerifiedDate = DateTime.UtcNow;
        patientInsurance.VerificationStatus = response.Status;
        await _context.SaveChangesAsync();

        return response;
    }

    #endregion

    #region Insurance Claims

    public async Task<IEnumerable<InsuranceClaimDto>> GetClaimsAsync(int branchId, ClaimStatus? status = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Set<InsuranceClaim>()
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Plan)
            .Include(c => c.Patient)
            .Include(c => c.Invoice)
            .Include(c => c.Items)
            .Where(c => c.BranchId == branchId);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.ServiceDateFrom >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(c => c.ServiceDateTo <= endDate.Value);
        }

        var claims = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        return claims.Select(MapToClaimDto);
    }

    public async Task<IEnumerable<InsuranceClaimDto>> GetClaimsByPatientAsync(int patientId)
    {
        var claims = await _context.Set<InsuranceClaim>()
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Plan)
            .Include(c => c.Invoice)
            .Include(c => c.Items)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return claims.Select(MapToClaimDto);
    }

    public async Task<InsuranceClaimDto?> GetClaimByIdAsync(int id)
    {
        var claim = await _context.Set<InsuranceClaim>()
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Plan)
            .Include(c => c.Patient)
            .Include(c => c.Invoice)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        return claim != null ? MapToClaimDto(claim) : null;
    }

    public async Task<InsuranceClaimDto?> GetClaimByNumberAsync(string claimNumber)
    {
        var claim = await _context.Set<InsuranceClaim>()
            .AsNoTracking()
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Plan)
            .Include(c => c.Patient)
            .Include(c => c.Invoice)
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClaimNumber == claimNumber);

        return claim != null ? MapToClaimDto(claim) : null;
    }

    public async Task<InsuranceClaimDto> CreateClaimAsync(int branchId, CreateInsuranceClaimDto dto)
    {
        var claimNumber = await GenerateClaimNumberAsync(branchId);

        var claim = new InsuranceClaim
        {
            BranchId = branchId,
            ClaimNumber = claimNumber,
            PatientInsuranceId = dto.PatientInsuranceId,
            PatientId = dto.PatientId,
            InvoiceId = dto.InvoiceId,
            PreAuthorizationId = dto.PreAuthorizationId,
            Status = ClaimStatus.Draft,
            ClaimType = dto.ClaimType,
            ServiceDateFrom = dto.ServiceDateFrom,
            ServiceDateTo = dto.ServiceDateTo,
            RenderingProviderName = dto.RenderingProviderName,
            RenderingProviderNpi = dto.RenderingProviderNpi,
            FacilityName = dto.FacilityName,
            PlaceOfServiceCode = dto.PlaceOfServiceCode,
            PrimaryDiagnosisCode = dto.PrimaryDiagnosisCode,
            SecondaryDiagnosisCodes = dto.SecondaryDiagnosisCodes != null
                ? JsonSerializer.Serialize(dto.SecondaryDiagnosisCodes)
                : null,
            InternalNotes = dto.InternalNotes,
            CreatedAt = DateTime.UtcNow
        };

        // Add claim items
        decimal totalBilled = 0;
        int lineNumber = 1;
        foreach (var itemDto in dto.Items)
        {
            var billedAmount = itemDto.UnitPrice * itemDto.Units;
            totalBilled += billedAmount;

            claim.Items.Add(new InsuranceClaimItem
            {
                BranchId = branchId,
                LineNumber = lineNumber++,
                ProcedureCode = itemDto.ProcedureCode,
                ProcedureDescription = itemDto.ProcedureDescription,
                Modifiers = itemDto.Modifiers,
                DiagnosisPointers = itemDto.DiagnosisPointers,
                RevenueCode = itemDto.RevenueCode,
                NdcCode = itemDto.NdcCode,
                ServiceDate = itemDto.ServiceDate,
                Units = itemDto.Units,
                UnitType = itemDto.UnitType,
                UnitPrice = itemDto.UnitPrice,
                BilledAmount = billedAmount,
                Notes = itemDto.Notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        claim.TotalBilledAmount = totalBilled;

        // Set filing deadline based on plan
        var patientInsurance = await _context.Set<PatientInsurance>()
            .Include(pi => pi.Plan)
            .FirstOrDefaultAsync(pi => pi.Id == dto.PatientInsuranceId);

        if (patientInsurance?.Plan != null)
        {
            claim.FilingDeadline = dto.ServiceDateTo.AddDays(patientInsurance.Plan.ClaimGracePeriodDays);
        }

        _context.Set<InsuranceClaim>().Add(claim);
        await _context.SaveChangesAsync();

        return MapToClaimDto(claim);
    }

    public async Task<InsuranceClaimDto> UpdateClaimAsync(int id, CreateInsuranceClaimDto dto)
    {
        var claim = await _context.Set<InsuranceClaim>()
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (claim == null)
        {
            throw new KeyNotFoundException($"Claim with ID {id} not found");
        }

        if (claim.Status != ClaimStatus.Draft)
        {
            throw new InvalidOperationException("Only draft claims can be updated");
        }

        claim.ClaimType = dto.ClaimType;
        claim.ServiceDateFrom = dto.ServiceDateFrom;
        claim.ServiceDateTo = dto.ServiceDateTo;
        claim.RenderingProviderName = dto.RenderingProviderName;
        claim.RenderingProviderNpi = dto.RenderingProviderNpi;
        claim.FacilityName = dto.FacilityName;
        claim.PlaceOfServiceCode = dto.PlaceOfServiceCode;
        claim.PrimaryDiagnosisCode = dto.PrimaryDiagnosisCode;
        claim.SecondaryDiagnosisCodes = dto.SecondaryDiagnosisCodes != null
            ? JsonSerializer.Serialize(dto.SecondaryDiagnosisCodes)
            : null;
        claim.InternalNotes = dto.InternalNotes;
        claim.UpdatedAt = DateTime.UtcNow;

        // Update items
        _context.Set<InsuranceClaimItem>().RemoveRange(claim.Items);

        decimal totalBilled = 0;
        int lineNumber = 1;
        foreach (var itemDto in dto.Items)
        {
            var billedAmount = itemDto.UnitPrice * itemDto.Units;
            totalBilled += billedAmount;

            claim.Items.Add(new InsuranceClaimItem
            {
                BranchId = claim.BranchId,
                ClaimId = claim.Id,
                LineNumber = lineNumber++,
                ProcedureCode = itemDto.ProcedureCode,
                ProcedureDescription = itemDto.ProcedureDescription,
                Modifiers = itemDto.Modifiers,
                DiagnosisPointers = itemDto.DiagnosisPointers,
                RevenueCode = itemDto.RevenueCode,
                NdcCode = itemDto.NdcCode,
                ServiceDate = itemDto.ServiceDate,
                Units = itemDto.Units,
                UnitType = itemDto.UnitType,
                UnitPrice = itemDto.UnitPrice,
                BilledAmount = billedAmount,
                Notes = itemDto.Notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        claim.TotalBilledAmount = totalBilled;

        await _context.SaveChangesAsync();

        return MapToClaimDto(claim);
    }

    public async Task<ClaimSubmissionResponseDto> SubmitClaimAsync(SubmitClaimDto dto)
    {
        var claim = await _context.Set<InsuranceClaim>()
            .Include(c => c.Items)
            .Include(c => c.PatientInsurance).ThenInclude(pi => pi.Provider)
            .FirstOrDefaultAsync(c => c.Id == dto.ClaimId);

        if (claim == null)
        {
            return new ClaimSubmissionResponseDto
            {
                Success = false,
                Errors = new List<string> { "Claim not found" }
            };
        }

        var errors = new List<string>();
        var warnings = new List<string>();

        // Validate claim
        if (string.IsNullOrEmpty(claim.PrimaryDiagnosisCode))
        {
            errors.Add("Primary diagnosis code is required");
        }

        if (!claim.Items.Any())
        {
            errors.Add("At least one claim item is required");
        }

        if (claim.FilingDeadline.HasValue && claim.FilingDeadline.Value < DateTime.UtcNow)
        {
            warnings.Add("Filing deadline has passed");
        }

        if (dto.ValidateOnly)
        {
            return new ClaimSubmissionResponseDto
            {
                Success = !errors.Any(),
                ClaimNumber = claim.ClaimNumber,
                Errors = errors,
                Warnings = warnings
            };
        }

        if (errors.Any())
        {
            return new ClaimSubmissionResponseDto
            {
                Success = false,
                ClaimNumber = claim.ClaimNumber,
                Errors = errors,
                Warnings = warnings
            };
        }

        // Submit claim (in production, this would call the insurance provider's API)
        claim.Status = ClaimStatus.Submitted;
        claim.SubmissionDate = DateTime.UtcNow;
        claim.EdiClaimId = $"EDI-{Guid.NewGuid():N}"[..20].ToUpper();
        claim.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ClaimSubmissionResponseDto
        {
            Success = true,
            ClaimNumber = claim.ClaimNumber,
            EdiClaimId = claim.EdiClaimId,
            SubmissionDate = claim.SubmissionDate,
            Warnings = warnings
        };
    }

    public async Task<ClaimStatusResponseDto> CheckClaimStatusAsync(int claimId)
    {
        var claim = await _context.Set<InsuranceClaim>().FindAsync(claimId);
        if (claim == null)
        {
            throw new KeyNotFoundException($"Claim with ID {claimId} not found");
        }

        // In production, this would call the insurance provider's status API
        return new ClaimStatusResponseDto
        {
            ClaimNumber = claim.ClaimNumber,
            Status = claim.Status,
            StatusDate = claim.UpdatedAt ?? claim.CreatedAt,
            ApprovedAmount = claim.ApprovedAmount,
            PaidAmount = claim.PaidAmount,
            DenialReason = claim.DenialReason,
            PayerRemarks = claim.InsuranceRemarks
        };
    }

    public async Task VoidClaimAsync(int claimId, string reason)
    {
        var claim = await _context.Set<InsuranceClaim>().FindAsync(claimId);
        if (claim != null)
        {
            claim.Status = ClaimStatus.Voided;
            claim.InternalNotes = $"{claim.InternalNotes}\nVoided: {reason}";
            claim.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ClaimSubmissionResponseDto> ResubmitClaimAsync(int claimId)
    {
        var claim = await _context.Set<InsuranceClaim>().FindAsync(claimId);
        if (claim == null)
        {
            return new ClaimSubmissionResponseDto
            {
                Success = false,
                Errors = new List<string> { "Claim not found" }
            };
        }

        if (claim.Status != ClaimStatus.Denied)
        {
            return new ClaimSubmissionResponseDto
            {
                Success = false,
                Errors = new List<string> { "Only denied claims can be resubmitted" }
            };
        }

        claim.Status = ClaimStatus.Resubmitted;
        claim.ResubmissionCount++;
        claim.OriginalClaimId = claim.Id;
        claim.SubmissionDate = DateTime.UtcNow;
        claim.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new ClaimSubmissionResponseDto
        {
            Success = true,
            ClaimNumber = claim.ClaimNumber,
            SubmissionDate = claim.SubmissionDate
        };
    }

    public async Task<string> GenerateClaimNumberAsync(int branchId)
    {
        var prefix = "CLM";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = await _context.Set<InsuranceClaim>()
            .Where(c => c.BranchId == branchId && c.ClaimNumber.StartsWith($"{prefix}-{branchId}-{date}"))
            .CountAsync() + 1;

        return $"{prefix}-{branchId}-{date}-{sequence:D4}";
    }

    #endregion

    #region Pre-Authorization

    public async Task<IEnumerable<InsurancePreAuthorizationDto>> GetPreAuthorizationsAsync(int branchId, PreAuthStatus? status = null)
    {
        var query = _context.Set<InsurancePreAuthorization>()
            .AsNoTracking()
            .Include(pa => pa.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(pa => pa.Patient)
            .Where(pa => pa.BranchId == branchId);

        if (status.HasValue)
        {
            query = query.Where(pa => pa.Status == status.Value);
        }

        var preAuths = await query.OrderByDescending(pa => pa.CreatedAt).ToListAsync();

        return preAuths.Select(MapToPreAuthDto);
    }

    public async Task<IEnumerable<InsurancePreAuthorizationDto>> GetPreAuthorizationsByPatientAsync(int patientId)
    {
        var preAuths = await _context.Set<InsurancePreAuthorization>()
            .AsNoTracking()
            .Include(pa => pa.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Where(pa => pa.PatientId == patientId)
            .OrderByDescending(pa => pa.CreatedAt)
            .ToListAsync();

        return preAuths.Select(MapToPreAuthDto);
    }

    public async Task<InsurancePreAuthorizationDto?> GetPreAuthorizationByIdAsync(int id)
    {
        var preAuth = await _context.Set<InsurancePreAuthorization>()
            .AsNoTracking()
            .Include(pa => pa.PatientInsurance).ThenInclude(pi => pi.Provider)
            .Include(pa => pa.Patient)
            .FirstOrDefaultAsync(pa => pa.Id == id);

        return preAuth != null ? MapToPreAuthDto(preAuth) : null;
    }

    public async Task<InsurancePreAuthorizationDto> CreatePreAuthorizationAsync(int branchId, CreatePreAuthorizationDto dto)
    {
        var preAuthNumber = await GeneratePreAuthNumberAsync(branchId);

        var preAuth = new InsurancePreAuthorization
        {
            BranchId = branchId,
            PreAuthNumber = preAuthNumber,
            PatientInsuranceId = dto.PatientInsuranceId,
            PatientId = dto.PatientId,
            Status = PreAuthStatus.Draft,
            RequestType = dto.RequestType,
            ServiceCategory = dto.ServiceCategory,
            RequestedProcedures = JsonSerializer.Serialize(dto.RequestedProcedures),
            PrimaryDiagnosisCode = dto.PrimaryDiagnosisCode,
            PrimaryDiagnosisDescription = dto.PrimaryDiagnosisDescription,
            SecondaryDiagnosisCodes = dto.SecondaryDiagnosisCodes != null
                ? JsonSerializer.Serialize(dto.SecondaryDiagnosisCodes)
                : null,
            ClinicalJustification = dto.ClinicalJustification,
            RequestingProviderName = dto.RequestingProviderName,
            RequestingProviderNpi = dto.RequestingProviderNpi,
            PlannedServiceDate = dto.PlannedServiceDate,
            RequestedDays = dto.RequestedDays,
            RequestedUnits = dto.RequestedUnits,
            EstimatedCost = dto.EstimatedCost,
            AttachedDocuments = dto.AttachedDocuments != null
                ? JsonSerializer.Serialize(dto.AttachedDocuments)
                : null,
            InternalNotes = dto.InternalNotes,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<InsurancePreAuthorization>().Add(preAuth);
        await _context.SaveChangesAsync();

        return MapToPreAuthDto(preAuth);
    }

    public async Task<InsurancePreAuthorizationDto> UpdatePreAuthorizationAsync(int id, CreatePreAuthorizationDto dto)
    {
        var preAuth = await _context.Set<InsurancePreAuthorization>().FindAsync(id);
        if (preAuth == null)
        {
            throw new KeyNotFoundException($"Pre-authorization with ID {id} not found");
        }

        if (preAuth.Status != PreAuthStatus.Draft)
        {
            throw new InvalidOperationException("Only draft pre-authorizations can be updated");
        }

        preAuth.RequestType = dto.RequestType;
        preAuth.ServiceCategory = dto.ServiceCategory;
        preAuth.RequestedProcedures = JsonSerializer.Serialize(dto.RequestedProcedures);
        preAuth.PrimaryDiagnosisCode = dto.PrimaryDiagnosisCode;
        preAuth.PrimaryDiagnosisDescription = dto.PrimaryDiagnosisDescription;
        preAuth.SecondaryDiagnosisCodes = dto.SecondaryDiagnosisCodes != null
            ? JsonSerializer.Serialize(dto.SecondaryDiagnosisCodes)
            : null;
        preAuth.ClinicalJustification = dto.ClinicalJustification;
        preAuth.RequestingProviderName = dto.RequestingProviderName;
        preAuth.RequestingProviderNpi = dto.RequestingProviderNpi;
        preAuth.PlannedServiceDate = dto.PlannedServiceDate;
        preAuth.RequestedDays = dto.RequestedDays;
        preAuth.RequestedUnits = dto.RequestedUnits;
        preAuth.EstimatedCost = dto.EstimatedCost;
        preAuth.AttachedDocuments = dto.AttachedDocuments != null
            ? JsonSerializer.Serialize(dto.AttachedDocuments)
            : null;
        preAuth.InternalNotes = dto.InternalNotes;
        preAuth.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToPreAuthDto(preAuth);
    }

    public async Task<InsurancePreAuthorizationDto> SubmitPreAuthorizationAsync(int id)
    {
        var preAuth = await _context.Set<InsurancePreAuthorization>().FindAsync(id);
        if (preAuth == null)
        {
            throw new KeyNotFoundException($"Pre-authorization with ID {id} not found");
        }

        preAuth.Status = PreAuthStatus.Submitted;
        preAuth.SubmissionDate = DateTime.UtcNow;
        preAuth.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToPreAuthDto(preAuth);
    }

    public async Task CancelPreAuthorizationAsync(int id, string reason)
    {
        var preAuth = await _context.Set<InsurancePreAuthorization>().FindAsync(id);
        if (preAuth != null)
        {
            preAuth.Status = PreAuthStatus.Cancelled;
            preAuth.InternalNotes = $"{preAuth.InternalNotes}\nCancelled: {reason}";
            preAuth.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GeneratePreAuthNumberAsync(int branchId)
    {
        var prefix = "PA";
        var date = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequence = await _context.Set<InsurancePreAuthorization>()
            .Where(pa => pa.BranchId == branchId && pa.PreAuthNumber.StartsWith($"{prefix}-{branchId}-{date}"))
            .CountAsync() + 1;

        return $"{prefix}-{branchId}-{date}-{sequence:D4}";
    }

    #endregion

    #region Statistics

    public async Task<InsuranceStatisticsDto> GetStatisticsAsync(int branchId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var providers = await _context.Set<InsuranceProvider>()
            .Where(p => p.BranchId == branchId)
            .ToListAsync();

        var plans = await _context.Set<InsurancePlan>()
            .Where(p => p.BranchId == branchId)
            .ToListAsync();

        var patientInsurances = await _context.Set<PatientInsurance>()
            .Where(pi => pi.BranchId == branchId)
            .ToListAsync();

        var claimsQuery = _context.Set<InsuranceClaim>()
            .Where(c => c.BranchId == branchId);

        if (startDate.HasValue)
        {
            claimsQuery = claimsQuery.Where(c => c.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            claimsQuery = claimsQuery.Where(c => c.CreatedAt <= endDate.Value);
        }

        var claims = await claimsQuery.ToListAsync();

        var preAuthsQuery = _context.Set<InsurancePreAuthorization>()
            .Where(pa => pa.BranchId == branchId);

        if (startDate.HasValue)
        {
            preAuthsQuery = preAuthsQuery.Where(pa => pa.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            preAuthsQuery = preAuthsQuery.Where(pa => pa.CreatedAt <= endDate.Value);
        }

        var preAuths = await preAuthsQuery.ToListAsync();

        return new InsuranceStatisticsDto
        {
            TotalProviders = providers.Count,
            ActiveProviders = providers.Count(p => p.IsActive),
            TotalPlans = plans.Count,
            ActivePlans = plans.Count(p => p.IsActive),
            TotalPatientInsurances = patientInsurances.Count,
            ActivePatientInsurances = patientInsurances.Count(pi => pi.IsActive),

            TotalClaims = claims.Count,
            DraftClaims = claims.Count(c => c.Status == ClaimStatus.Draft),
            SubmittedClaims = claims.Count(c => c.Status == ClaimStatus.Submitted),
            ApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
            DeniedClaims = claims.Count(c => c.Status == ClaimStatus.Denied),
            PaidClaims = claims.Count(c => c.Status == ClaimStatus.Paid),
            TotalBilledAmount = claims.Sum(c => c.TotalBilledAmount),
            TotalApprovedAmount = claims.Sum(c => c.ApprovedAmount),
            TotalPaidAmount = claims.Sum(c => c.PaidAmount),
            TotalDeniedAmount = claims.Where(c => c.Status == ClaimStatus.Denied).Sum(c => c.TotalBilledAmount),
            ClaimApprovalRate = claims.Count > 0
                ? (decimal)claims.Count(c => c.Status == ClaimStatus.Approved || c.Status == ClaimStatus.Paid) / claims.Count * 100
                : 0,

            TotalPreAuths = preAuths.Count,
            PendingPreAuths = preAuths.Count(pa => pa.Status == PreAuthStatus.Submitted || pa.Status == PreAuthStatus.InReview),
            ApprovedPreAuths = preAuths.Count(pa => pa.Status == PreAuthStatus.Approved),
            DeniedPreAuths = preAuths.Count(pa => pa.Status == PreAuthStatus.Denied),
            PreAuthApprovalRate = preAuths.Count > 0
                ? (decimal)preAuths.Count(pa => pa.Status == PreAuthStatus.Approved) / preAuths.Count * 100
                : 0
        };
    }

    #endregion

    #region Private Mapping Methods

    private static InsuranceProviderDto MapToProviderDto(InsuranceProvider provider)
    {
        return new InsuranceProviderDto
        {
            Id = provider.Id,
            BranchId = provider.BranchId,
            ProviderCode = provider.ProviderCode,
            Name = provider.Name,
            NameAr = provider.NameAr,
            ShortName = provider.ShortName,
            InsuranceType = provider.InsuranceType,
            IsActive = provider.IsActive,
            ContactPerson = provider.ContactPerson,
            Email = provider.Email,
            Phone = provider.Phone,
            Website = provider.Website,
            Address = provider.Address,
            City = provider.City,
            Country = provider.Country,
            PaymentTermsDays = provider.PaymentTermsDays,
            DefaultDiscountPercent = provider.DefaultDiscountPercent,
            PlansCount = provider.Plans?.Count ?? 0,
            CreatedAt = provider.CreatedAt,
            UpdatedAt = provider.UpdatedAt
        };
    }

    private static InsurancePlanDto MapToPlanDto(InsurancePlan plan)
    {
        return new InsurancePlanDto
        {
            Id = plan.Id,
            BranchId = plan.BranchId,
            ProviderId = plan.ProviderId,
            ProviderName = plan.Provider?.Name,
            PlanCode = plan.PlanCode,
            Name = plan.Name,
            NameAr = plan.NameAr,
            Description = plan.Description,
            PlanType = plan.PlanType,
            NetworkType = plan.NetworkType,
            IsActive = plan.IsActive,
            CoveragePercent = plan.CoveragePercent,
            CopayPercent = plan.CopayPercent,
            CopayAmount = plan.CopayAmount,
            DeductibleAmount = plan.DeductibleAmount,
            AnnualMaximum = plan.AnnualMaximum,
            LifetimeMaximum = plan.LifetimeMaximum,
            MaxPerVisit = plan.MaxPerVisit,
            RequiresPreAuth = plan.RequiresPreAuth,
            RequiresReferral = plan.RequiresReferral,
            WaitingPeriodDays = plan.WaitingPeriodDays,
            ClaimGracePeriodDays = plan.ClaimGracePeriodDays,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        };
    }

    private static PatientInsuranceDto MapToPatientInsuranceDto(PatientInsurance pi)
    {
        return new PatientInsuranceDto
        {
            Id = pi.Id,
            BranchId = pi.BranchId,
            PatientId = pi.PatientId,
            PatientName = pi.Patient != null ? $"{pi.Patient.FirstName} {pi.Patient.LastName}" : null,
            ProviderId = pi.ProviderId,
            ProviderName = pi.Provider?.Name,
            PlanId = pi.PlanId,
            PlanName = pi.Plan?.Name,
            MemberId = pi.MemberId,
            GroupNumber = pi.GroupNumber,
            SubscriberId = pi.SubscriberId,
            IsPrimary = pi.IsPrimary,
            RelationshipToSubscriber = pi.RelationshipToSubscriber,
            SubscriberName = pi.SubscriberName,
            EffectiveDate = pi.EffectiveDate,
            TerminationDate = pi.TerminationDate,
            IsActive = pi.IsActive,
            LastVerifiedDate = pi.LastVerifiedDate,
            VerificationStatus = pi.VerificationStatus,
            RemainingDeductible = pi.RemainingDeductible,
            RemainingAnnualBenefit = pi.RemainingAnnualBenefit,
            YtdClaimsAmount = pi.YtdClaimsAmount,
            CreatedAt = pi.CreatedAt,
            UpdatedAt = pi.UpdatedAt
        };
    }

    private static InsuranceClaimDto MapToClaimDto(InsuranceClaim claim)
    {
        return new InsuranceClaimDto
        {
            Id = claim.Id,
            BranchId = claim.BranchId,
            ClaimNumber = claim.ClaimNumber,
            PatientInsuranceId = claim.PatientInsuranceId,
            InsuranceProviderName = claim.PatientInsurance?.Provider?.Name,
            InsurancePlanName = claim.PatientInsurance?.Plan?.Name,
            MemberId = claim.PatientInsurance?.MemberId,
            PatientId = claim.PatientId,
            PatientName = claim.Patient != null ? $"{claim.Patient.FirstName} {claim.Patient.LastName}" : null,
            InvoiceId = claim.InvoiceId,
            InvoiceNumber = claim.Invoice?.InvoiceNumber,
            PreAuthorizationId = claim.PreAuthorizationId,
            PreAuthNumber = claim.PreAuthorization?.PreAuthNumber,
            Status = claim.Status,
            ClaimType = claim.ClaimType,
            ServiceDateFrom = claim.ServiceDateFrom,
            ServiceDateTo = claim.ServiceDateTo,
            SubmissionDate = claim.SubmissionDate,
            TotalBilledAmount = claim.TotalBilledAmount,
            ApprovedAmount = claim.ApprovedAmount,
            PatientResponsibility = claim.PatientResponsibility,
            CopayAmount = claim.CopayAmount,
            DeductibleAmount = claim.DeductibleAmount,
            CoinsuranceAmount = claim.CoinsuranceAmount,
            AdjustmentAmount = claim.AdjustmentAmount,
            PaidAmount = claim.PaidAmount,
            PaymentDate = claim.PaymentDate,
            PaymentReference = claim.PaymentReference,
            RenderingProviderName = claim.RenderingProviderName,
            PrimaryDiagnosisCode = claim.PrimaryDiagnosisCode,
            DenialReasonCode = claim.DenialReasonCode,
            DenialReason = claim.DenialReason,
            ItemsCount = claim.Items?.Count ?? 0,
            ResubmissionCount = claim.ResubmissionCount,
            FilingDeadline = claim.FilingDeadline,
            CreatedAt = claim.CreatedAt,
            UpdatedAt = claim.UpdatedAt
        };
    }

    private static InsurancePreAuthorizationDto MapToPreAuthDto(InsurancePreAuthorization pa)
    {
        return new InsurancePreAuthorizationDto
        {
            Id = pa.Id,
            BranchId = pa.BranchId,
            PreAuthNumber = pa.PreAuthNumber,
            PatientInsuranceId = pa.PatientInsuranceId,
            InsuranceProviderName = pa.PatientInsurance?.Provider?.Name,
            MemberId = pa.PatientInsurance?.MemberId,
            PatientId = pa.PatientId,
            PatientName = pa.Patient != null ? $"{pa.Patient.FirstName} {pa.Patient.LastName}" : null,
            Status = pa.Status,
            RequestType = pa.RequestType,
            ServiceCategory = pa.ServiceCategory,
            PrimaryDiagnosisCode = pa.PrimaryDiagnosisCode,
            PrimaryDiagnosisDescription = pa.PrimaryDiagnosisDescription,
            RequestingProviderName = pa.RequestingProviderName,
            PlannedServiceDate = pa.PlannedServiceDate,
            RequestedUnits = pa.RequestedUnits,
            EstimatedCost = pa.EstimatedCost,
            SubmissionDate = pa.SubmissionDate,
            AuthorizationNumber = pa.AuthorizationNumber,
            ApprovedAmount = pa.ApprovedAmount,
            ApprovedUnits = pa.ApprovedUnits,
            ApprovalDate = pa.ApprovalDate,
            EffectiveDate = pa.EffectiveDate,
            ExpirationDate = pa.ExpirationDate,
            DenialReasonCode = pa.DenialReasonCode,
            DenialReason = pa.DenialReason,
            FollowUpDate = pa.FollowUpDate,
            AssignedTo = pa.AssignedTo,
            CreatedAt = pa.CreatedAt,
            UpdatedAt = pa.UpdatedAt
        };
    }

    #endregion
}

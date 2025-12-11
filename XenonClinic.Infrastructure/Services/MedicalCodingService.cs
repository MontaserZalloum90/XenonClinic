using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.DTOs;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;

namespace XenonClinic.Infrastructure.Services;

/// <summary>
/// Service for Medical Coding (ICD-10, CPT, HCPCS) operations
/// </summary>
public class MedicalCodingService : IMedicalCodingService
{
    private readonly ClinicDbContext _context;

    public MedicalCodingService(ClinicDbContext context)
    {
        _context = context;
    }

    #region ICD-10 Codes

    public async Task<IEnumerable<ICD10CodeDto>> SearchICD10CodesAsync(int branchId, ICD10SearchRequestDto request)
    {
        var query = _context.ICD10Codes
            .Where(c => c.BranchId == branchId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(searchTerm) ||
                c.ShortDescription.ToLower().Contains(searchTerm) ||
                (c.LongDescription != null && c.LongDescription.ToLower().Contains(searchTerm)) ||
                (c.SearchKeywords != null && c.SearchKeywords.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.CategoryCode))
            query = query.Where(c => c.CategoryCode == request.CategoryCode);

        if (request.ChapterNumber.HasValue)
            query = query.Where(c => c.ChapterNumber == request.ChapterNumber.Value);

        if (request.IsBillable.HasValue)
            query = query.Where(c => c.IsBillable == request.IsBillable.Value);

        if (request.IsCommon.HasValue)
            query = query.Where(c => c.IsCommon == request.IsCommon.Value);

        if (!string.IsNullOrWhiteSpace(request.Specialty))
            query = query.Where(c => c.SpecialtyTags != null && c.SpecialtyTags.Contains(request.Specialty));

        var skip = (request.PageNumber - 1) * request.PageSize;

        var codes = await query
            .OrderBy(c => c.Code)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return codes.Select(MapToICD10Dto);
    }

    public async Task<ICD10CodeDto?> GetICD10CodeByIdAsync(int id)
    {
        var code = await _context.ICD10Codes.FindAsync(id);
        return code == null ? null : MapToICD10Dto(code);
    }

    public async Task<ICD10CodeDto?> GetICD10CodeByCodeAsync(int branchId, string code)
    {
        var entity = await _context.ICD10Codes
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == code && c.IsActive);
        return entity == null ? null : MapToICD10Dto(entity);
    }

    public async Task<IEnumerable<ICD10CodeDto>> GetCommonICD10CodesAsync(int branchId, string? specialty = null, int limit = 50)
    {
        var query = _context.ICD10Codes
            .Where(c => c.BranchId == branchId && c.IsActive && c.IsCommon);

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(c => c.SpecialtyTags != null && c.SpecialtyTags.Contains(specialty));

        var codes = await query
            .OrderBy(c => c.Code)
            .Take(limit)
            .ToListAsync();

        return codes.Select(MapToICD10Dto);
    }

    public async Task<ICD10CodeDto> CreateICD10CodeAsync(int branchId, CreateICD10CodeDto dto)
    {
        var entity = new ICD10Code
        {
            BranchId = branchId,
            Code = dto.Code,
            ShortDescription = dto.ShortDescription,
            LongDescription = dto.LongDescription,
            DescriptionAr = dto.DescriptionAr,
            CategoryCode = dto.CategoryCode,
            CategoryDescription = dto.CategoryDescription,
            ChapterNumber = dto.ChapterNumber,
            ChapterTitle = dto.ChapterTitle,
            BlockRange = dto.BlockRange,
            BlockDescription = dto.BlockDescription,
            CodeType = dto.CodeType,
            IsBillable = dto.IsBillable,
            IsActive = true,
            VersionYear = dto.VersionYear,
            EffectiveDate = dto.EffectiveDate,
            ParentCode = dto.ParentCode,
            IncludesNotes = dto.IncludesNotes,
            Excludes1Notes = dto.Excludes1Notes,
            Excludes2Notes = dto.Excludes2Notes,
            CodeFirstNotes = dto.CodeFirstNotes,
            UseAdditionalCodeNotes = dto.UseAdditionalCodeNotes,
            IsCommon = dto.IsCommon,
            SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null,
            SearchKeywords = dto.SearchKeywords,
            CreatedAt = DateTime.UtcNow
        };

        _context.ICD10Codes.Add(entity);
        await _context.SaveChangesAsync();

        return MapToICD10Dto(entity);
    }

    public async Task<ICD10CodeDto> UpdateICD10CodeAsync(int id, CreateICD10CodeDto dto)
    {
        var entity = await _context.ICD10Codes.FindAsync(id)
            ?? throw new KeyNotFoundException($"ICD-10 code with ID {id} not found");

        entity.Code = dto.Code;
        entity.ShortDescription = dto.ShortDescription;
        entity.LongDescription = dto.LongDescription;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.CategoryCode = dto.CategoryCode;
        entity.CategoryDescription = dto.CategoryDescription;
        entity.ChapterNumber = dto.ChapterNumber;
        entity.ChapterTitle = dto.ChapterTitle;
        entity.BlockRange = dto.BlockRange;
        entity.BlockDescription = dto.BlockDescription;
        entity.CodeType = dto.CodeType;
        entity.IsBillable = dto.IsBillable;
        entity.VersionYear = dto.VersionYear;
        entity.EffectiveDate = dto.EffectiveDate;
        entity.ParentCode = dto.ParentCode;
        entity.IncludesNotes = dto.IncludesNotes;
        entity.Excludes1Notes = dto.Excludes1Notes;
        entity.Excludes2Notes = dto.Excludes2Notes;
        entity.CodeFirstNotes = dto.CodeFirstNotes;
        entity.UseAdditionalCodeNotes = dto.UseAdditionalCodeNotes;
        entity.IsCommon = dto.IsCommon;
        entity.SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null;
        entity.SearchKeywords = dto.SearchKeywords;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToICD10Dto(entity);
    }

    public async Task DeleteICD10CodeAsync(int id)
    {
        var entity = await _context.ICD10Codes.FindAsync(id)
            ?? throw new KeyNotFoundException($"ICD-10 code with ID {id} not found");

        entity.IsActive = false;
        entity.TerminationDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<int> ImportICD10CodesAsync(int branchId, Stream fileStream, string fileType)
    {
        var importedCount = 0;

        using var reader = new StreamReader(fileStream);

        if (fileType.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            // Skip header
            await reader.ReadLineAsync();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    var code = parts[0].Trim().Trim('"');
                    var shortDesc = parts[1].Trim().Trim('"');
                    var isBillable = parts.Length > 2 && parts[2].Trim().Equals("1", StringComparison.OrdinalIgnoreCase);

                    var existing = await _context.ICD10Codes
                        .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == code);

                    if (existing == null)
                    {
                        var entity = new ICD10Code
                        {
                            BranchId = branchId,
                            Code = code,
                            ShortDescription = shortDesc,
                            CategoryCode = code.Length >= 3 ? code[..3] : code,
                            ChapterNumber = DetermineICD10Chapter(code),
                            IsBillable = isBillable,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ICD10Codes.Add(entity);
                        importedCount++;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        return importedCount;
    }

    public Task<IEnumerable<ICD10ChapterDto>> GetICD10ChaptersAsync()
    {
        var chapters = new List<ICD10ChapterDto>
        {
            new() { ChapterNumber = 1, Title = "Certain infectious and parasitic diseases", CodeRange = "A00-B99" },
            new() { ChapterNumber = 2, Title = "Neoplasms", CodeRange = "C00-D49" },
            new() { ChapterNumber = 3, Title = "Diseases of the blood and blood-forming organs", CodeRange = "D50-D89" },
            new() { ChapterNumber = 4, Title = "Endocrine, nutritional and metabolic diseases", CodeRange = "E00-E89" },
            new() { ChapterNumber = 5, Title = "Mental, Behavioral and Neurodevelopmental disorders", CodeRange = "F01-F99" },
            new() { ChapterNumber = 6, Title = "Diseases of the nervous system", CodeRange = "G00-G99" },
            new() { ChapterNumber = 7, Title = "Diseases of the eye and adnexa", CodeRange = "H00-H59" },
            new() { ChapterNumber = 8, Title = "Diseases of the ear and mastoid process", CodeRange = "H60-H95" },
            new() { ChapterNumber = 9, Title = "Diseases of the circulatory system", CodeRange = "I00-I99" },
            new() { ChapterNumber = 10, Title = "Diseases of the respiratory system", CodeRange = "J00-J99" },
            new() { ChapterNumber = 11, Title = "Diseases of the digestive system", CodeRange = "K00-K95" },
            new() { ChapterNumber = 12, Title = "Diseases of the skin and subcutaneous tissue", CodeRange = "L00-L99" },
            new() { ChapterNumber = 13, Title = "Diseases of the musculoskeletal system and connective tissue", CodeRange = "M00-M99" },
            new() { ChapterNumber = 14, Title = "Diseases of the genitourinary system", CodeRange = "N00-N99" },
            new() { ChapterNumber = 15, Title = "Pregnancy, childbirth and the puerperium", CodeRange = "O00-O9A" },
            new() { ChapterNumber = 16, Title = "Certain conditions originating in the perinatal period", CodeRange = "P00-P96" },
            new() { ChapterNumber = 17, Title = "Congenital malformations, deformations and chromosomal abnormalities", CodeRange = "Q00-Q99" },
            new() { ChapterNumber = 18, Title = "Symptoms, signs and abnormal clinical and laboratory findings", CodeRange = "R00-R99" },
            new() { ChapterNumber = 19, Title = "Injury, poisoning and certain other consequences of external causes", CodeRange = "S00-T88" },
            new() { ChapterNumber = 20, Title = "External causes of morbidity", CodeRange = "V00-Y99" },
            new() { ChapterNumber = 21, Title = "Factors influencing health status and contact with health services", CodeRange = "Z00-Z99" },
            new() { ChapterNumber = 22, Title = "Codes for special purposes", CodeRange = "U00-U85" }
        };

        return Task.FromResult<IEnumerable<ICD10ChapterDto>>(chapters);
    }

    #endregion

    #region CPT Codes

    public async Task<IEnumerable<CPTCodeDto>> SearchCPTCodesAsync(int branchId, CPTSearchRequestDto request)
    {
        var query = _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(searchTerm) ||
                c.ShortDescription.ToLower().Contains(searchTerm) ||
                (c.LongDescription != null && c.LongDescription.ToLower().Contains(searchTerm)) ||
                (c.SearchKeywords != null && c.SearchKeywords.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(c => c.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.Subcategory))
            query = query.Where(c => c.Subcategory == request.Subcategory);

        if (!string.IsNullOrWhiteSpace(request.CodeType))
            query = query.Where(c => c.CodeType == request.CodeType);

        if (request.IsCommon.HasValue)
            query = query.Where(c => c.IsCommon == request.IsCommon.Value);

        if (request.IsAddOnCode.HasValue)
            query = query.Where(c => c.IsAddOnCode == request.IsAddOnCode.Value);

        if (!string.IsNullOrWhiteSpace(request.Specialty))
            query = query.Where(c => c.SpecialtyTags != null && c.SpecialtyTags.Contains(request.Specialty));

        if (request.MinFee.HasValue)
            query = query.Where(c => c.StandardFee >= request.MinFee.Value);

        if (request.MaxFee.HasValue)
            query = query.Where(c => c.StandardFee <= request.MaxFee.Value);

        var skip = (request.PageNumber - 1) * request.PageSize;

        var codes = await query
            .OrderBy(c => c.Code)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return codes.Select(MapToCPTDto);
    }

    public async Task<CPTCodeDto?> GetCPTCodeByIdAsync(int id)
    {
        var code = await _context.CPTCodes.FindAsync(id);
        return code == null ? null : MapToCPTDto(code);
    }

    public async Task<CPTCodeDto?> GetCPTCodeByCodeAsync(int branchId, string code)
    {
        var entity = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == code && c.IsActive);
        return entity == null ? null : MapToCPTDto(entity);
    }

    public async Task<IEnumerable<CPTCodeDto>> GetCommonCPTCodesAsync(int branchId, string? specialty = null, int limit = 50)
    {
        var query = _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive && c.IsCommon);

        if (!string.IsNullOrWhiteSpace(specialty))
            query = query.Where(c => c.SpecialtyTags != null && c.SpecialtyTags.Contains(specialty));

        var codes = await query
            .OrderBy(c => c.Code)
            .Take(limit)
            .ToListAsync();

        return codes.Select(MapToCPTDto);
    }

    public async Task<CPTCodeDto> CreateCPTCodeAsync(int branchId, CreateCPTCodeDto dto)
    {
        var entity = new CPTCode
        {
            BranchId = branchId,
            Code = dto.Code,
            ShortDescription = dto.ShortDescription,
            MediumDescription = dto.MediumDescription,
            LongDescription = dto.LongDescription,
            DescriptionAr = dto.DescriptionAr,
            Category = dto.Category,
            Subcategory = dto.Subcategory,
            Section = dto.Section,
            CodeRange = dto.CodeRange,
            CodeType = dto.CodeType,
            IsActive = true,
            VersionYear = dto.VersionYear,
            EffectiveDate = dto.EffectiveDate,
            GlobalPeriod = dto.GlobalPeriod,
            WorkRvu = dto.WorkRvu,
            FacilityPeRvu = dto.FacilityPeRvu,
            NonFacilityPeRvu = dto.NonFacilityPeRvu,
            MalpracticeRvu = dto.MalpracticeRvu,
            TotalFacilityRvu = (dto.WorkRvu ?? 0) + (dto.FacilityPeRvu ?? 0) + (dto.MalpracticeRvu ?? 0),
            TotalNonFacilityRvu = (dto.WorkRvu ?? 0) + (dto.NonFacilityPeRvu ?? 0) + (dto.MalpracticeRvu ?? 0),
            StandardFee = dto.StandardFee,
            MedicareFee = dto.MedicareFee,
            ProfessionalFee = dto.ProfessionalFee,
            TechnicalFee = dto.TechnicalFee,
            TypicalTime = dto.TypicalTime,
            PreServiceTime = dto.PreServiceTime,
            IntraServiceTime = dto.IntraServiceTime,
            PostServiceTime = dto.PostServiceTime,
            CommonModifiers = dto.CommonModifiers != null ? JsonSerializer.Serialize(dto.CommonModifiers) : null,
            RequiresModifier = dto.RequiresModifier,
            IsAddOnCode = dto.IsAddOnCode,
            PrimaryCode = dto.PrimaryCode,
            RelatedCodes = dto.RelatedCodes != null ? JsonSerializer.Serialize(dto.RelatedCodes) : null,
            ExcludedCodes = dto.ExcludedCodes != null ? JsonSerializer.Serialize(dto.ExcludedCodes) : null,
            BundledCodes = dto.BundledCodes != null ? JsonSerializer.Serialize(dto.BundledCodes) : null,
            CommonDiagnosisCodes = dto.CommonDiagnosisCodes != null ? JsonSerializer.Serialize(dto.CommonDiagnosisCodes) : null,
            Notes = dto.Notes,
            ClinicalExamples = dto.ClinicalExamples,
            IsCommon = dto.IsCommon,
            SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null,
            SearchKeywords = dto.SearchKeywords,
            CreatedAt = DateTime.UtcNow
        };

        _context.CPTCodes.Add(entity);
        await _context.SaveChangesAsync();

        return MapToCPTDto(entity);
    }

    public async Task<CPTCodeDto> UpdateCPTCodeAsync(int id, CreateCPTCodeDto dto)
    {
        var entity = await _context.CPTCodes.FindAsync(id)
            ?? throw new KeyNotFoundException($"CPT code with ID {id} not found");

        entity.Code = dto.Code;
        entity.ShortDescription = dto.ShortDescription;
        entity.MediumDescription = dto.MediumDescription;
        entity.LongDescription = dto.LongDescription;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.Category = dto.Category;
        entity.Subcategory = dto.Subcategory;
        entity.Section = dto.Section;
        entity.CodeRange = dto.CodeRange;
        entity.CodeType = dto.CodeType;
        entity.VersionYear = dto.VersionYear;
        entity.EffectiveDate = dto.EffectiveDate;
        entity.GlobalPeriod = dto.GlobalPeriod;
        entity.WorkRvu = dto.WorkRvu;
        entity.FacilityPeRvu = dto.FacilityPeRvu;
        entity.NonFacilityPeRvu = dto.NonFacilityPeRvu;
        entity.MalpracticeRvu = dto.MalpracticeRvu;
        entity.TotalFacilityRvu = (dto.WorkRvu ?? 0) + (dto.FacilityPeRvu ?? 0) + (dto.MalpracticeRvu ?? 0);
        entity.TotalNonFacilityRvu = (dto.WorkRvu ?? 0) + (dto.NonFacilityPeRvu ?? 0) + (dto.MalpracticeRvu ?? 0);
        entity.StandardFee = dto.StandardFee;
        entity.MedicareFee = dto.MedicareFee;
        entity.ProfessionalFee = dto.ProfessionalFee;
        entity.TechnicalFee = dto.TechnicalFee;
        entity.TypicalTime = dto.TypicalTime;
        entity.PreServiceTime = dto.PreServiceTime;
        entity.IntraServiceTime = dto.IntraServiceTime;
        entity.PostServiceTime = dto.PostServiceTime;
        entity.CommonModifiers = dto.CommonModifiers != null ? JsonSerializer.Serialize(dto.CommonModifiers) : null;
        entity.RequiresModifier = dto.RequiresModifier;
        entity.IsAddOnCode = dto.IsAddOnCode;
        entity.PrimaryCode = dto.PrimaryCode;
        entity.RelatedCodes = dto.RelatedCodes != null ? JsonSerializer.Serialize(dto.RelatedCodes) : null;
        entity.ExcludedCodes = dto.ExcludedCodes != null ? JsonSerializer.Serialize(dto.ExcludedCodes) : null;
        entity.BundledCodes = dto.BundledCodes != null ? JsonSerializer.Serialize(dto.BundledCodes) : null;
        entity.CommonDiagnosisCodes = dto.CommonDiagnosisCodes != null ? JsonSerializer.Serialize(dto.CommonDiagnosisCodes) : null;
        entity.Notes = dto.Notes;
        entity.ClinicalExamples = dto.ClinicalExamples;
        entity.IsCommon = dto.IsCommon;
        entity.SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null;
        entity.SearchKeywords = dto.SearchKeywords;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToCPTDto(entity);
    }

    public async Task DeleteCPTCodeAsync(int id)
    {
        var entity = await _context.CPTCodes.FindAsync(id)
            ?? throw new KeyNotFoundException($"CPT code with ID {id} not found");

        entity.IsActive = false;
        entity.TerminationDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<int> ImportCPTCodesAsync(int branchId, Stream fileStream, string fileType)
    {
        var importedCount = 0;

        using var reader = new StreamReader(fileStream);

        if (fileType.Equals("csv", StringComparison.OrdinalIgnoreCase))
        {
            // Skip header
            await reader.ReadLineAsync();

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    var code = parts[0].Trim().Trim('"');
                    var shortDesc = parts[1].Trim().Trim('"');

                    var existing = await _context.CPTCodes
                        .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == code);

                    if (existing == null)
                    {
                        var entity = new CPTCode
                        {
                            BranchId = branchId,
                            Code = code,
                            ShortDescription = shortDesc,
                            Category = DetermineCPTCategory(code),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.CPTCodes.Add(entity);
                        importedCount++;
                    }
                }
            }
        }

        await _context.SaveChangesAsync();
        return importedCount;
    }

    public Task<IEnumerable<CPTCategoryDto>> GetCPTCategoriesAsync()
    {
        var categories = new List<CPTCategoryDto>
        {
            new() { CategoryName = "Evaluation and Management", Description = "E/M Services", CodeRange = "99202-99499" },
            new() { CategoryName = "Anesthesia", Description = "Anesthesia Services", CodeRange = "00100-01999" },
            new() { CategoryName = "Surgery", Description = "Surgical Procedures", CodeRange = "10004-69990",
                Subcategories = new List<CPTSubcategoryDto>
                {
                    new() { SubcategoryName = "Integumentary System", CodeRange = "10004-19499" },
                    new() { SubcategoryName = "Musculoskeletal System", CodeRange = "20100-29999" },
                    new() { SubcategoryName = "Respiratory System", CodeRange = "30000-32999" },
                    new() { SubcategoryName = "Cardiovascular System", CodeRange = "33010-37799" },
                    new() { SubcategoryName = "Digestive System", CodeRange = "40490-49999" },
                    new() { SubcategoryName = "Urinary System", CodeRange = "50010-53899" },
                    new() { SubcategoryName = "Nervous System", CodeRange = "61000-64999" },
                    new() { SubcategoryName = "Eye and Ocular Adnexa", CodeRange = "65091-68899" },
                }
            },
            new() { CategoryName = "Radiology", Description = "Diagnostic Imaging", CodeRange = "70010-79999" },
            new() { CategoryName = "Pathology and Laboratory", Description = "Laboratory Services", CodeRange = "80047-89398" },
            new() { CategoryName = "Medicine", Description = "Medicine Services", CodeRange = "90281-99607" }
        };

        return Task.FromResult<IEnumerable<CPTCategoryDto>>(categories);
    }

    public async Task<IEnumerable<CPTCodeDto>> GetAddOnCodesAsync(int branchId, string primaryCode)
    {
        var codes = await _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive && c.IsAddOnCode && c.PrimaryCode == primaryCode)
            .OrderBy(c => c.Code)
            .ToListAsync();

        return codes.Select(MapToCPTDto);
    }

    #endregion

    #region HCPCS Codes

    public async Task<IEnumerable<HCPCSCodeDto>> SearchHCPCSCodesAsync(int branchId, HCPCSSearchRequestDto request)
    {
        var query = _context.HCPCSCodes
            .Where(c => c.BranchId == branchId && c.IsActive);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Code.ToLower().Contains(searchTerm) ||
                c.ShortDescription.ToLower().Contains(searchTerm) ||
                (c.LongDescription != null && c.LongDescription.ToLower().Contains(searchTerm)) ||
                (c.SearchKeywords != null && c.SearchKeywords.ToLower().Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(c => c.Category == request.Category);

        if (!string.IsNullOrWhiteSpace(request.CodeType))
            query = query.Where(c => c.CodeType == request.CodeType);

        if (request.IsCommon.HasValue)
            query = query.Where(c => c.IsCommon == request.IsCommon.Value);

        if (!string.IsNullOrWhiteSpace(request.DrugName))
            query = query.Where(c => c.DrugName != null && c.DrugName.ToLower().Contains(request.DrugName.ToLower()));

        var skip = (request.PageNumber - 1) * request.PageSize;

        var codes = await query
            .OrderBy(c => c.Code)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return codes.Select(MapToHCPCSDto);
    }

    public async Task<HCPCSCodeDto?> GetHCPCSCodeByIdAsync(int id)
    {
        var code = await _context.HCPCSCodes.FindAsync(id);
        return code == null ? null : MapToHCPCSDto(code);
    }

    public async Task<HCPCSCodeDto?> GetHCPCSCodeByCodeAsync(int branchId, string code)
    {
        var entity = await _context.HCPCSCodes
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == code && c.IsActive);
        return entity == null ? null : MapToHCPCSDto(entity);
    }

    public async Task<HCPCSCodeDto> CreateHCPCSCodeAsync(int branchId, CreateHCPCSCodeDto dto)
    {
        var entity = new HCPCSCode
        {
            BranchId = branchId,
            Code = dto.Code,
            ShortDescription = dto.ShortDescription,
            LongDescription = dto.LongDescription,
            DescriptionAr = dto.DescriptionAr,
            Level = dto.Level,
            Category = dto.Category,
            Subcategory = dto.Subcategory,
            CodeType = dto.CodeType,
            IsActive = true,
            VersionYear = dto.VersionYear,
            EffectiveDate = dto.EffectiveDate,
            CoverageNotes = dto.CoverageNotes,
            BillingNotes = dto.BillingNotes,
            PricingIndicator = dto.PricingIndicator,
            StandardFee = dto.StandardFee,
            MedicareFee = dto.MedicareFee,
            NdcCode = dto.NdcCode,
            DrugName = dto.DrugName,
            DrugUnit = dto.DrugUnit,
            RouteOfAdministration = dto.RouteOfAdministration,
            CommonModifiers = dto.CommonModifiers != null ? JsonSerializer.Serialize(dto.CommonModifiers) : null,
            IsCommon = dto.IsCommon,
            SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null,
            SearchKeywords = dto.SearchKeywords,
            CreatedAt = DateTime.UtcNow
        };

        _context.HCPCSCodes.Add(entity);
        await _context.SaveChangesAsync();

        return MapToHCPCSDto(entity);
    }

    public async Task<HCPCSCodeDto> UpdateHCPCSCodeAsync(int id, CreateHCPCSCodeDto dto)
    {
        var entity = await _context.HCPCSCodes.FindAsync(id)
            ?? throw new KeyNotFoundException($"HCPCS code with ID {id} not found");

        entity.Code = dto.Code;
        entity.ShortDescription = dto.ShortDescription;
        entity.LongDescription = dto.LongDescription;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.Level = dto.Level;
        entity.Category = dto.Category;
        entity.Subcategory = dto.Subcategory;
        entity.CodeType = dto.CodeType;
        entity.VersionYear = dto.VersionYear;
        entity.EffectiveDate = dto.EffectiveDate;
        entity.CoverageNotes = dto.CoverageNotes;
        entity.BillingNotes = dto.BillingNotes;
        entity.PricingIndicator = dto.PricingIndicator;
        entity.StandardFee = dto.StandardFee;
        entity.MedicareFee = dto.MedicareFee;
        entity.NdcCode = dto.NdcCode;
        entity.DrugName = dto.DrugName;
        entity.DrugUnit = dto.DrugUnit;
        entity.RouteOfAdministration = dto.RouteOfAdministration;
        entity.CommonModifiers = dto.CommonModifiers != null ? JsonSerializer.Serialize(dto.CommonModifiers) : null;
        entity.IsCommon = dto.IsCommon;
        entity.SpecialtyTags = dto.SpecialtyTags != null ? JsonSerializer.Serialize(dto.SpecialtyTags) : null;
        entity.SearchKeywords = dto.SearchKeywords;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToHCPCSDto(entity);
    }

    public async Task DeleteHCPCSCodeAsync(int id)
    {
        var entity = await _context.HCPCSCodes.FindAsync(id)
            ?? throw new KeyNotFoundException($"HCPCS code with ID {id} not found");

        entity.IsActive = false;
        entity.TerminationDate = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Modifiers

    public async Task<IEnumerable<MedicalCodeModifierDto>> SearchModifiersAsync(int branchId, ModifierSearchRequestDto request)
    {
        var query = _context.MedicalCodeModifiers
            .Where(m => m.BranchId == branchId);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(m =>
                m.Code.ToLower().Contains(searchTerm) ||
                m.Description.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrWhiteSpace(request.ModifierType))
            query = query.Where(m => m.ModifierType == request.ModifierType);

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(m => m.Category == request.Category);

        if (request.IsActive.HasValue)
            query = query.Where(m => m.IsActive == request.IsActive.Value);

        var skip = (request.PageNumber - 1) * request.PageSize;

        var modifiers = await query
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync();

        return modifiers.Select(MapToModifierDto);
    }

    public async Task<MedicalCodeModifierDto?> GetModifierByIdAsync(int id)
    {
        var modifier = await _context.MedicalCodeModifiers.FindAsync(id);
        return modifier == null ? null : MapToModifierDto(modifier);
    }

    public async Task<MedicalCodeModifierDto?> GetModifierByCodeAsync(int branchId, string code)
    {
        var entity = await _context.MedicalCodeModifiers
            .FirstOrDefaultAsync(m => m.BranchId == branchId && m.Code == code && m.IsActive);
        return entity == null ? null : MapToModifierDto(entity);
    }

    public async Task<IEnumerable<MedicalCodeModifierDto>> GetCommonModifiersAsync(int branchId, string? procedureCode = null)
    {
        var query = _context.MedicalCodeModifiers
            .Where(m => m.BranchId == branchId && m.IsActive);

        if (!string.IsNullOrWhiteSpace(procedureCode))
            query = query.Where(m => m.ApplicableCodes == null || m.ApplicableCodes.Contains(procedureCode));

        var modifiers = await query
            .OrderBy(m => m.SortOrder)
            .Take(20)
            .ToListAsync();

        return modifiers.Select(MapToModifierDto);
    }

    public async Task<MedicalCodeModifierDto> CreateModifierAsync(int branchId, MedicalCodeModifierDto dto)
    {
        var entity = new MedicalCodeModifier
        {
            BranchId = branchId,
            Code = dto.Code,
            Description = dto.Description,
            DescriptionAr = dto.DescriptionAr,
            ModifierType = dto.ModifierType,
            Category = dto.Category,
            IsActive = dto.IsActive,
            PriceAdjustmentType = dto.PriceAdjustmentType,
            PriceAdjustmentValue = dto.PriceAdjustmentValue,
            AppliesToProfessional = dto.AppliesToProfessional,
            AppliesToTechnical = dto.AppliesToTechnical,
            IsGlobalModifier = dto.IsGlobalModifier,
            IsInformational = dto.IsInformational,
            UsageNotes = dto.UsageNotes,
            SortOrder = dto.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.MedicalCodeModifiers.Add(entity);
        await _context.SaveChangesAsync();

        return MapToModifierDto(entity);
    }

    public async Task<MedicalCodeModifierDto> UpdateModifierAsync(int id, MedicalCodeModifierDto dto)
    {
        var entity = await _context.MedicalCodeModifiers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Modifier with ID {id} not found");

        entity.Code = dto.Code;
        entity.Description = dto.Description;
        entity.DescriptionAr = dto.DescriptionAr;
        entity.ModifierType = dto.ModifierType;
        entity.Category = dto.Category;
        entity.IsActive = dto.IsActive;
        entity.PriceAdjustmentType = dto.PriceAdjustmentType;
        entity.PriceAdjustmentValue = dto.PriceAdjustmentValue;
        entity.AppliesToProfessional = dto.AppliesToProfessional;
        entity.AppliesToTechnical = dto.AppliesToTechnical;
        entity.IsGlobalModifier = dto.IsGlobalModifier;
        entity.IsInformational = dto.IsInformational;
        entity.UsageNotes = dto.UsageNotes;
        entity.SortOrder = dto.SortOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToModifierDto(entity);
    }

    public async Task DeleteModifierAsync(int id)
    {
        var entity = await _context.MedicalCodeModifiers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Modifier with ID {id} not found");

        entity.IsActive = false;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Validation

    public async Task<CodeValidationResponseDto> ValidateCodesAsync(CodeValidationRequestDto request)
    {
        var response = new CodeValidationResponseDto { IsValid = true };

        // Validate diagnosis code
        if (!string.IsNullOrWhiteSpace(request.DiagnosisCode))
        {
            var diagCode = await _context.ICD10Codes
                .FirstOrDefaultAsync(c => c.Code == request.DiagnosisCode && c.IsActive);

            if (diagCode == null)
            {
                response.IsValid = false;
                response.Issues.Add(new CodeValidationIssueDto
                {
                    Severity = "Error",
                    Code = request.DiagnosisCode,
                    Message = $"ICD-10 code '{request.DiagnosisCode}' not found or inactive",
                    RuleId = "ICD10_NOT_FOUND"
                });
            }
            else if (!diagCode.IsBillable)
            {
                response.IsValid = false;
                response.Issues.Add(new CodeValidationIssueDto
                {
                    Severity = "Error",
                    Code = request.DiagnosisCode,
                    Message = $"ICD-10 code '{request.DiagnosisCode}' is not billable",
                    RuleId = "ICD10_NOT_BILLABLE"
                });

                // Suggest more specific codes
                var specificCodes = await _context.ICD10Codes
                    .Where(c => c.ParentCode == request.DiagnosisCode && c.IsBillable && c.IsActive)
                    .Take(5)
                    .ToListAsync();

                foreach (var code in specificCodes)
                {
                    response.Suggestions.Add(new CodeSuggestionDto
                    {
                        Code = code.Code,
                        Description = code.ShortDescription,
                        SuggestionType = "MoreSpecific",
                        Reason = "This code is more specific and billable"
                    });
                }
            }
        }

        // Validate procedure code
        if (!string.IsNullOrWhiteSpace(request.ProcedureCode))
        {
            var procCode = await _context.CPTCodes
                .FirstOrDefaultAsync(c => c.Code == request.ProcedureCode && c.IsActive);

            if (procCode == null)
            {
                response.IsValid = false;
                response.Issues.Add(new CodeValidationIssueDto
                {
                    Severity = "Error",
                    Code = request.ProcedureCode,
                    Message = $"CPT code '{request.ProcedureCode}' not found or inactive",
                    RuleId = "CPT_NOT_FOUND"
                });
            }
            else
            {
                // Check if modifier is required
                if (procCode.RequiresModifier && (request.Modifiers == null || !request.Modifiers.Any()))
                {
                    response.IsValid = false;
                    response.Issues.Add(new CodeValidationIssueDto
                    {
                        Severity = "Error",
                        Code = request.ProcedureCode,
                        Message = $"CPT code '{request.ProcedureCode}' requires a modifier",
                        RuleId = "MODIFIER_REQUIRED"
                    });
                }

                // Check add-on code rules
                if (procCode.IsAddOnCode)
                {
                    response.Issues.Add(new CodeValidationIssueDto
                    {
                        Severity = "Warning",
                        Code = request.ProcedureCode,
                        Message = $"CPT code '{request.ProcedureCode}' is an add-on code and must be billed with primary code '{procCode.PrimaryCode}'",
                        RuleId = "ADDON_CODE"
                    });
                }
            }
        }

        // Validate modifiers
        if (request.Modifiers != null)
        {
            foreach (var modifier in request.Modifiers)
            {
                var mod = await _context.MedicalCodeModifiers
                    .FirstOrDefaultAsync(m => m.Code == modifier && m.IsActive);

                if (mod == null)
                {
                    response.IsValid = false;
                    response.Issues.Add(new CodeValidationIssueDto
                    {
                        Severity = "Error",
                        Code = modifier,
                        Message = $"Modifier '{modifier}' not found or inactive",
                        RuleId = "MODIFIER_NOT_FOUND"
                    });
                }
            }
        }

        return response;
    }

    public async Task<bool> IsBillableCodeAsync(string code)
    {
        var icd10 = await _context.ICD10Codes
            .FirstOrDefaultAsync(c => c.Code == code && c.IsActive);

        return icd10?.IsBillable ?? false;
    }

    public async Task<IEnumerable<ICD10CodeDto>> GetRelatedDiagnosisCodesAsync(string procedureCode)
    {
        var cptCode = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.Code == procedureCode && c.IsActive);

        if (cptCode?.CommonDiagnosisCodes == null)
            return Enumerable.Empty<ICD10CodeDto>();

        var diagnosisCodes = JsonSerializer.Deserialize<List<string>>(cptCode.CommonDiagnosisCodes) ?? new List<string>();

        var icd10Codes = await _context.ICD10Codes
            .Where(c => diagnosisCodes.Contains(c.Code) && c.IsActive)
            .ToListAsync();

        return icd10Codes.Select(MapToICD10Dto);
    }

    public async Task<IEnumerable<string>> GetBundledCodesAsync(string procedureCode)
    {
        var cptCode = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.Code == procedureCode && c.IsActive);

        if (cptCode?.BundledCodes == null)
            return Enumerable.Empty<string>();

        return JsonSerializer.Deserialize<List<string>>(cptCode.BundledCodes) ?? new List<string>();
    }

    #endregion

    #region Fee Lookup

    public async Task<FeeLookupResponseDto> LookupFeeAsync(int branchId, FeeLookupRequestDto request)
    {
        var cptCode = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == request.ProcedureCode && c.IsActive);

        if (cptCode == null)
            throw new KeyNotFoundException($"CPT code '{request.ProcedureCode}' not found");

        var response = new FeeLookupResponseDto
        {
            ProcedureCode = cptCode.Code,
            Description = cptCode.ShortDescription,
            StandardFee = cptCode.StandardFee ?? 0,
            FacilityFee = cptCode.StandardFee, // Use standard fee if facility-specific not available
            NonFacilityFee = cptCode.StandardFee,
            ProfessionalFee = cptCode.ProfessionalFee,
            TechnicalFee = cptCode.TechnicalFee,
            FeeScheduleUsed = request.FeeSchedule ?? "Standard",
            FeeEffectiveDate = cptCode.EffectiveDate ?? DateTime.UtcNow,
            AppliedModifiers = new List<string>(),
            Adjustments = new List<FeeAdjustmentDto>()
        };

        // Apply modifier adjustments
        if (request.Modifiers != null && request.Modifiers.Any())
        {
            decimal adjustedFee = response.StandardFee;

            foreach (var modCode in request.Modifiers)
            {
                var modifier = await _context.MedicalCodeModifiers
                    .FirstOrDefaultAsync(m => m.BranchId == branchId && m.Code == modCode && m.IsActive);

                if (modifier != null && !modifier.IsInformational)
                {
                    response.AppliedModifiers.Add(modCode);

                    if (modifier.PriceAdjustmentType == "Percentage" && modifier.PriceAdjustmentValue.HasValue)
                    {
                        var adjustment = adjustedFee * (modifier.PriceAdjustmentValue.Value / 100);
                        adjustedFee += adjustment;

                        response.Adjustments.Add(new FeeAdjustmentDto
                        {
                            AdjustmentType = "Modifier",
                            Description = $"Modifier {modCode}: {modifier.Description}",
                            AdjustmentPercent = modifier.PriceAdjustmentValue.Value,
                            AdjustmentAmount = adjustment
                        });
                    }
                    else if (modifier.PriceAdjustmentType == "Fixed" && modifier.PriceAdjustmentValue.HasValue)
                    {
                        adjustedFee += modifier.PriceAdjustmentValue.Value;

                        response.Adjustments.Add(new FeeAdjustmentDto
                        {
                            AdjustmentType = "Modifier",
                            Description = $"Modifier {modCode}: {modifier.Description}",
                            AdjustmentPercent = 0,
                            AdjustmentAmount = modifier.PriceAdjustmentValue.Value
                        });
                    }
                }
            }

            response.AdjustedFee = adjustedFee;
        }
        else
        {
            response.AdjustedFee = response.StandardFee;
        }

        return response;
    }

    public async Task<IEnumerable<FeeScheduleEntryDto>> GetFeeScheduleAsync(int branchId, string? feeScheduleName = null)
    {
        // Return CPT codes as fee schedule entries
        var query = _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive && c.StandardFee.HasValue);

        var codes = await query
            .OrderBy(c => c.Code)
            .Take(1000)
            .ToListAsync();

        return codes.Select(c => new FeeScheduleEntryDto
        {
            Id = c.Id,
            ProcedureCode = c.Code,
            ProcedureDescription = c.ShortDescription,
            FeeScheduleName = feeScheduleName ?? "Standard",
            EffectiveDate = c.EffectiveDate ?? DateTime.UtcNow,
            TerminationDate = c.TerminationDate,
            StandardFee = c.StandardFee ?? 0,
            FacilityFee = c.StandardFee,
            NonFacilityFee = c.StandardFee,
            ProfessionalFee = c.ProfessionalFee,
            TechnicalFee = c.TechnicalFee,
            IsActive = c.IsActive
        });
    }

    public async Task<FeeScheduleEntryDto> UpdateFeeScheduleEntryAsync(int branchId, FeeScheduleEntryDto dto)
    {
        var cptCode = await _context.CPTCodes
            .FirstOrDefaultAsync(c => c.BranchId == branchId && c.Code == dto.ProcedureCode)
            ?? throw new KeyNotFoundException($"CPT code '{dto.ProcedureCode}' not found");

        cptCode.StandardFee = dto.StandardFee;
        cptCode.ProfessionalFee = dto.ProfessionalFee;
        cptCode.TechnicalFee = dto.TechnicalFee;
        cptCode.EffectiveDate = dto.EffectiveDate;
        cptCode.TerminationDate = dto.TerminationDate;
        cptCode.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return dto;
    }

    #endregion

    #region Statistics

    public async Task<MedicalCodingStatisticsDto> GetStatisticsAsync(int branchId)
    {
        var stats = new MedicalCodingStatisticsDto();

        // ICD-10 statistics
        stats.TotalICD10Codes = await _context.ICD10Codes.CountAsync(c => c.BranchId == branchId);
        stats.ActiveICD10Codes = await _context.ICD10Codes.CountAsync(c => c.BranchId == branchId && c.IsActive);
        stats.BillableICD10Codes = await _context.ICD10Codes.CountAsync(c => c.BranchId == branchId && c.IsActive && c.IsBillable);

        // CPT statistics
        stats.TotalCPTCodes = await _context.CPTCodes.CountAsync(c => c.BranchId == branchId);
        stats.ActiveCPTCodes = await _context.CPTCodes.CountAsync(c => c.BranchId == branchId && c.IsActive);

        // HCPCS statistics
        stats.TotalHCPCSCodes = await _context.HCPCSCodes.CountAsync(c => c.BranchId == branchId);
        stats.ActiveHCPCSCodes = await _context.HCPCSCodes.CountAsync(c => c.BranchId == branchId && c.IsActive);

        // Modifier statistics
        stats.TotalModifiers = await _context.MedicalCodeModifiers.CountAsync(m => m.BranchId == branchId);
        stats.ActiveModifiers = await _context.MedicalCodeModifiers.CountAsync(m => m.BranchId == branchId && m.IsActive);

        // ICD-10 by chapter
        var icd10ByChapter = await _context.ICD10Codes
            .Where(c => c.BranchId == branchId && c.IsActive)
            .GroupBy(c => c.ChapterNumber)
            .Select(g => new { Chapter = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.ICD10ByChapter = icd10ByChapter.ToDictionary(x => $"Chapter {x.Chapter}", x => x.Count);

        // CPT by category
        var cptByCategory = await _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive)
            .GroupBy(c => c.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToListAsync();

        stats.CPTByCategory = cptByCategory.ToDictionary(x => x.Category, x => x.Count);

        return stats;
    }

    public async Task<IEnumerable<ICD10CodeDto>> GetMostUsedDiagnosisCodesAsync(int branchId, int limit = 20)
    {
        // Return common codes as most used (in a real scenario, this would query usage data)
        var codes = await _context.ICD10Codes
            .Where(c => c.BranchId == branchId && c.IsActive && c.IsCommon)
            .OrderBy(c => c.Code)
            .Take(limit)
            .ToListAsync();

        return codes.Select(MapToICD10Dto);
    }

    public async Task<IEnumerable<CPTCodeDto>> GetMostUsedProcedureCodesAsync(int branchId, int limit = 20)
    {
        // Return common codes as most used
        var codes = await _context.CPTCodes
            .Where(c => c.BranchId == branchId && c.IsActive && c.IsCommon)
            .OrderBy(c => c.Code)
            .Take(limit)
            .ToListAsync();

        return codes.Select(MapToCPTDto);
    }

    #endregion

    #region Private Helper Methods

    private static ICD10CodeDto MapToICD10Dto(ICD10Code entity)
    {
        return new ICD10CodeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ShortDescription = entity.ShortDescription,
            LongDescription = entity.LongDescription,
            DescriptionAr = entity.DescriptionAr,
            CategoryCode = entity.CategoryCode,
            CategoryDescription = entity.CategoryDescription,
            ChapterNumber = entity.ChapterNumber,
            ChapterTitle = entity.ChapterTitle,
            BlockRange = entity.BlockRange,
            BlockDescription = entity.BlockDescription,
            CodeType = entity.CodeType,
            IsBillable = entity.IsBillable,
            IsActive = entity.IsActive,
            VersionYear = entity.VersionYear,
            EffectiveDate = entity.EffectiveDate,
            TerminationDate = entity.TerminationDate,
            ParentCode = entity.ParentCode,
            IsCommon = entity.IsCommon,
            SpecialtyTags = !string.IsNullOrEmpty(entity.SpecialtyTags)
                ? JsonSerializer.Deserialize<List<string>>(entity.SpecialtyTags)
                : null
        };
    }

    private static CPTCodeDto MapToCPTDto(CPTCode entity)
    {
        return new CPTCodeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ShortDescription = entity.ShortDescription,
            MediumDescription = entity.MediumDescription,
            LongDescription = entity.LongDescription,
            DescriptionAr = entity.DescriptionAr,
            Category = entity.Category,
            Subcategory = entity.Subcategory,
            Section = entity.Section,
            CodeRange = entity.CodeRange,
            CodeType = entity.CodeType,
            IsActive = entity.IsActive,
            VersionYear = entity.VersionYear,
            EffectiveDate = entity.EffectiveDate,
            GlobalPeriod = entity.GlobalPeriod,
            WorkRvu = entity.WorkRvu,
            TotalFacilityRvu = entity.TotalFacilityRvu,
            TotalNonFacilityRvu = entity.TotalNonFacilityRvu,
            StandardFee = entity.StandardFee,
            MedicareFee = entity.MedicareFee,
            ProfessionalFee = entity.ProfessionalFee,
            TechnicalFee = entity.TechnicalFee,
            TypicalTime = entity.TypicalTime,
            CommonModifiers = !string.IsNullOrEmpty(entity.CommonModifiers)
                ? JsonSerializer.Deserialize<List<string>>(entity.CommonModifiers)
                : null,
            RequiresModifier = entity.RequiresModifier,
            IsAddOnCode = entity.IsAddOnCode,
            PrimaryCode = entity.PrimaryCode,
            RelatedCodes = !string.IsNullOrEmpty(entity.RelatedCodes)
                ? JsonSerializer.Deserialize<List<string>>(entity.RelatedCodes)
                : null,
            CommonDiagnosisCodes = !string.IsNullOrEmpty(entity.CommonDiagnosisCodes)
                ? JsonSerializer.Deserialize<List<string>>(entity.CommonDiagnosisCodes)
                : null,
            Notes = entity.Notes,
            IsCommon = entity.IsCommon,
            SpecialtyTags = !string.IsNullOrEmpty(entity.SpecialtyTags)
                ? JsonSerializer.Deserialize<List<string>>(entity.SpecialtyTags)
                : null
        };
    }

    private static HCPCSCodeDto MapToHCPCSDto(HCPCSCode entity)
    {
        return new HCPCSCodeDto
        {
            Id = entity.Id,
            Code = entity.Code,
            ShortDescription = entity.ShortDescription,
            LongDescription = entity.LongDescription,
            DescriptionAr = entity.DescriptionAr,
            Level = entity.Level,
            Category = entity.Category,
            Subcategory = entity.Subcategory,
            CodeType = entity.CodeType,
            IsActive = entity.IsActive,
            VersionYear = entity.VersionYear,
            EffectiveDate = entity.EffectiveDate,
            CoverageNotes = entity.CoverageNotes,
            BillingNotes = entity.BillingNotes,
            StandardFee = entity.StandardFee,
            MedicareFee = entity.MedicareFee,
            NdcCode = entity.NdcCode,
            DrugName = entity.DrugName,
            DrugUnit = entity.DrugUnit,
            RouteOfAdministration = entity.RouteOfAdministration,
            CommonModifiers = !string.IsNullOrEmpty(entity.CommonModifiers)
                ? JsonSerializer.Deserialize<List<string>>(entity.CommonModifiers)
                : null,
            IsCommon = entity.IsCommon,
            SpecialtyTags = !string.IsNullOrEmpty(entity.SpecialtyTags)
                ? JsonSerializer.Deserialize<List<string>>(entity.SpecialtyTags)
                : null
        };
    }

    private static MedicalCodeModifierDto MapToModifierDto(MedicalCodeModifier entity)
    {
        return new MedicalCodeModifierDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            DescriptionAr = entity.DescriptionAr,
            ModifierType = entity.ModifierType,
            Category = entity.Category,
            IsActive = entity.IsActive,
            PriceAdjustmentType = entity.PriceAdjustmentType,
            PriceAdjustmentValue = entity.PriceAdjustmentValue,
            AppliesToProfessional = entity.AppliesToProfessional,
            AppliesToTechnical = entity.AppliesToTechnical,
            IsGlobalModifier = entity.IsGlobalModifier,
            IsInformational = entity.IsInformational,
            UsageNotes = entity.UsageNotes,
            SortOrder = entity.SortOrder
        };
    }

    private static int DetermineICD10Chapter(string code)
    {
        if (string.IsNullOrEmpty(code)) return 0;

        var firstChar = char.ToUpper(code[0]);
        return firstChar switch
        {
            'A' or 'B' => 1,
            'C' => 2,
            'D' when code.Length >= 2 && int.TryParse(code[1].ToString(), out var d) && d < 5 => 2,
            'D' => 3,
            'E' => 4,
            'F' => 5,
            'G' => 6,
            'H' when code.Length >= 2 && int.TryParse(code[1].ToString(), out var h) && h < 6 => 7,
            'H' => 8,
            'I' => 9,
            'J' => 10,
            'K' => 11,
            'L' => 12,
            'M' => 13,
            'N' => 14,
            'O' => 15,
            'P' => 16,
            'Q' => 17,
            'R' => 18,
            'S' or 'T' => 19,
            'V' or 'W' or 'X' or 'Y' => 20,
            'Z' => 21,
            'U' => 22,
            _ => 0
        };
    }

    private static string DetermineCPTCategory(string code)
    {
        if (!int.TryParse(code, out var codeNum)) return "Unknown";

        return codeNum switch
        {
            >= 99202 and <= 99499 => "Evaluation and Management",
            >= 100 and <= 1999 => "Anesthesia",
            >= 10004 and <= 69990 => "Surgery",
            >= 70010 and <= 79999 => "Radiology",
            >= 80047 and <= 89398 => "Pathology and Laboratory",
            >= 90281 and <= 99607 => "Medicine",
            _ => "Unknown"
        };
    }

    #endregion
}

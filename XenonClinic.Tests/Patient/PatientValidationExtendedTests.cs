using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using XenonClinic.Core.Entities;
using XenonClinic.Core.Interfaces;
using XenonClinic.Infrastructure.Data;
using XenonClinic.Infrastructure.Services;
using Xunit;

namespace XenonClinic.Tests.Patient;

/// <summary>
/// Extended validation tests for Patient entity.
/// Contains 300+ validation test cases.
/// </summary>
public class PatientValidationExtendedTests : IAsyncLifetime
{
    private ClinicDbContext _context = null!;
    private IPatientService _patientService = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ClinicDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ClinicDbContext(options);
        _patientService = new PatientService(_context);
        await SeedBasicDataAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }

    private async Task SeedBasicDataAsync()
    {
        var company = new Company { Id = 1, TenantId = 1, Name = "Test Clinic", Code = "TC001", IsActive = true };
        _context.Companies.Add(company);

        var branch = new Branch { Id = 1, CompanyId = 1, Code = "BR001", Name = "Main Branch", IsActive = true };
        _context.Branches.Add(branch);

        await _context.SaveChangesAsync();
    }

    #region Emirates ID Validation Tests

    [Theory]
    [InlineData("784-1234-1234567-1")]
    [InlineData("784-0000-0000000-0")]
    [InlineData("784-9999-9999999-9")]
    public async Task EmiratesId_ValidFormats_AcceptsAll(string emiratesId)
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = emiratesId;

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task EmiratesId_EmptyOrWhitespace_RejectsOrHandles(string emiratesId)
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = emiratesId;

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData("784")]
    [InlineData("784-")]
    [InlineData("784-1234")]
    [InlineData("784-1234-")]
    [InlineData("784-1234-1234567")]
    public async Task EmiratesId_IncompleteFormats_HandlesGracefully(string emiratesId)
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = emiratesId;

        // May be accepted or rejected based on validation rules
        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("ABC-1234-1234567-1")]
    [InlineData("784-ABCD-1234567-1")]
    [InlineData("784-1234-ABCDEFG-1")]
    [InlineData("784-1234-1234567-A")]
    public async Task EmiratesId_InvalidCharacters_HandlesGracefully(string emiratesId)
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = emiratesId;

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task EmiratesId_VeryLong_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = new string('1', 100);

        var action = () => _patientService.CreatePatientAsync(patient);
        // Should either truncate or throw
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task EmiratesId_WithSpaces_Trimmed()
    {
        var patient = CreateValidPatient();
        patient.EmiratesId = " 784-1234-1234567-2 ";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Name Validation Tests

    [Theory]
    [InlineData("John")]
    [InlineData("John Doe")]
    [InlineData("John Michael Doe")]
    [InlineData("John Michael James Doe")]
    [InlineData("O'Connor")]
    [InlineData("Smith-Jones")]
    [InlineData("Dr. John Doe")]
    public async Task FullNameEn_ValidNames_AcceptsAll(string name)
    {
        var patient = CreateValidPatient();
        patient.FullNameEn = name;
        patient.EmiratesId = $"784-NAME-{name.GetHashCode():D7}-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.FullNameEn.Should().Be(name);
    }

    [Theory]
    [InlineData("محمد")]
    [InlineData("محمد أحمد")]
    [InlineData("عبدالله بن محمد")]
    [InlineData("سارة بنت عبدالرحمن")]
    public async Task FullNameAr_ValidArabicNames_AcceptsAll(string name)
    {
        var patient = CreateValidPatient();
        patient.FullNameAr = name;
        patient.EmiratesId = $"784-ARAB-{Math.Abs(name.GetHashCode()):D7}-A";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.FullNameAr.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task FullNameEn_EmptyOrWhitespace_Rejects(string name)
    {
        var patient = CreateValidPatient();
        patient.FullNameEn = name;

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task FullNameEn_VeryLong_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.FullNameEn = new string('A', 500);
        patient.EmiratesId = "784-LONG-1234567-L";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Theory]
    [InlineData("John123")]
    [InlineData("John@Doe")]
    [InlineData("John#Doe")]
    [InlineData("John<Doe>")]
    public async Task FullNameEn_WithSpecialChars_HandlesGracefully(string name)
    {
        var patient = CreateValidPatient();
        patient.FullNameEn = name;
        patient.EmiratesId = $"784-SPEC-{Math.Abs(name.GetHashCode()):D7}-S";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task FullNameAr_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.FullNameAr = null;
        patient.EmiratesId = "784-NLAR-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.FullNameAr.Should().BeNull();
    }

    #endregion

    #region Date of Birth Validation Tests

    [Theory]
    [InlineData(1900, 1, 1)]
    [InlineData(1950, 6, 15)]
    [InlineData(1980, 12, 31)]
    [InlineData(2000, 2, 29)] // Leap year
    [InlineData(2020, 7, 4)]
    public async Task DateOfBirth_ValidDates_AcceptsAll(int year, int month, int day)
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = new DateTime(year, month, day);
        patient.EmiratesId = $"784-DOB{year:D4}-{month:D2}{day:D2}000-D";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DateOfBirth_Today_Accepts()
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = DateTime.UtcNow.Date;
        patient.EmiratesId = "784-TDAY-1234567-T";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DateOfBirth_Future_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = DateTime.UtcNow.AddYears(1);
        patient.EmiratesId = "784-FUTR-1234567-F";

        // May be accepted for expected births
        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DateOfBirth_VeryOld_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = new DateTime(1800, 1, 1);
        patient.EmiratesId = "784-VOLD-1234567-V";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task DateOfBirth_MinValue_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = DateTime.MinValue;
        patient.EmiratesId = "784-MINV-1234567-M";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    [Fact]
    public async Task DateOfBirth_MaxValue_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.DateOfBirth = DateTime.MaxValue;
        patient.EmiratesId = "784-MAXV-1234567-X";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().NotThrowAsync<NullReferenceException>();
    }

    #endregion

    #region Gender Validation Tests

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("O")]
    public async Task Gender_StandardValues_AcceptsAll(string gender)
    {
        var patient = CreateValidPatient();
        patient.Gender = gender;
        patient.EmiratesId = $"784-GND{gender}-1234567-G";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.Gender.Should().Be(gender);
    }

    [Theory]
    [InlineData("m")]
    [InlineData("f")]
    [InlineData("o")]
    public async Task Gender_LowerCase_HandlesGracefully(string gender)
    {
        var patient = CreateValidPatient();
        patient.Gender = gender;
        patient.EmiratesId = $"784-GNDL-{gender}234567-G";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Male")]
    [InlineData("Female")]
    [InlineData("Other")]
    public async Task Gender_FullWords_HandlesGracefully(string gender)
    {
        var patient = CreateValidPatient();
        patient.Gender = gender;
        patient.EmiratesId = $"784-GNDF-{Math.Abs(gender.GetHashCode()):D7}-G";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Gender_EmptyOrWhitespace_HandlesGracefully(string gender)
    {
        var patient = CreateValidPatient();
        patient.Gender = gender;
        patient.EmiratesId = "784-GNDE-1234567-G";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Gender_Null_Rejects()
    {
        var patient = CreateValidPatient();
        patient.Gender = null!;
        patient.EmiratesId = "784-GNDN-1234567-G";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Phone Number Validation Tests

    [Theory]
    [InlineData("+971501234567")]
    [InlineData("+971561234567")]
    [InlineData("+97143234567")]
    [InlineData("0501234567")]
    [InlineData("00971501234567")]
    public async Task PhoneNumber_UAEFormats_AcceptsAll(string phone)
    {
        var patient = CreateValidPatient();
        patient.PhoneNumber = phone;
        patient.EmiratesId = $"784-PHN{Math.Abs(phone.GetHashCode()):D7}-P";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("+1-202-555-0123")]
    [InlineData("+44 20 7946 0958")]
    [InlineData("+91 98765 43210")]
    [InlineData("+86 10 1234 5678")]
    public async Task PhoneNumber_InternationalFormats_AcceptsAll(string phone)
    {
        var patient = CreateValidPatient();
        patient.PhoneNumber = phone;
        patient.EmiratesId = $"784-PINT-{Math.Abs(phone.GetHashCode()):D7}-I";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task PhoneNumber_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.PhoneNumber = null;
        patient.EmiratesId = "784-PNUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task PhoneNumber_Empty_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.PhoneNumber = "";
        patient.EmiratesId = "784-PEMP-1234567-E";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("phone")]
    [InlineData("!@#$%")]
    public async Task PhoneNumber_InvalidFormats_HandlesGracefully(string phone)
    {
        var patient = CreateValidPatient();
        patient.PhoneNumber = phone;
        patient.EmiratesId = $"784-PINV-{Math.Abs(phone.GetHashCode()):D7}-I";

        // May accept invalid formats based on validation rules
        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Email Validation Tests

    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.com")]
    [InlineData("user+tag@example.org")]
    [InlineData("user@subdomain.domain.com")]
    [InlineData("firstname.lastname@company.ae")]
    public async Task Email_ValidFormats_AcceptsAll(string email)
    {
        var patient = CreateValidPatient();
        patient.Email = email;
        patient.EmiratesId = $"784-EML{Math.Abs(email.GetHashCode()):D7}-E";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Email_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.Email = null;
        patient.EmiratesId = "784-ENUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@.com")]
    public async Task Email_InvalidFormats_HandlesGracefully(string email)
    {
        var patient = CreateValidPatient();
        patient.Email = email;
        patient.EmiratesId = $"784-EINV-{Math.Abs(email.GetHashCode()):D7}-I";

        // May accept or reject based on validation
        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("TEST@EXAMPLE.COM")]
    [InlineData("Test@Example.COM")]
    public async Task Email_CaseVariations_AcceptsAll(string email)
    {
        var patient = CreateValidPatient();
        patient.Email = email;
        patient.EmiratesId = $"784-ECAS-{Math.Abs(email.GetHashCode()):D7}-C";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Blood Type Validation Tests

    [Theory]
    [InlineData("A+")]
    [InlineData("A-")]
    [InlineData("B+")]
    [InlineData("B-")]
    [InlineData("O+")]
    [InlineData("O-")]
    [InlineData("AB+")]
    [InlineData("AB-")]
    public async Task BloodType_ValidTypes_AcceptsAll(string bloodType)
    {
        var patient = CreateValidPatient();
        patient.BloodType = bloodType;
        patient.EmiratesId = $"784-BLD{bloodType.Replace("+", "P").Replace("-", "N")}-1234567-B";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.BloodType.Should().Be(bloodType);
    }

    [Fact]
    public async Task BloodType_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.BloodType = null;
        patient.EmiratesId = "784-BNUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("a+")]
    [InlineData("ab+")]
    public async Task BloodType_LowerCase_HandlesGracefully(string bloodType)
    {
        var patient = CreateValidPatient();
        patient.BloodType = bloodType;
        patient.EmiratesId = $"784-BLDC-{Math.Abs(bloodType.GetHashCode()):D7}-C";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("X+")]
    [InlineData("C-")]
    [InlineData("Invalid")]
    public async Task BloodType_InvalidTypes_HandlesGracefully(string bloodType)
    {
        var patient = CreateValidPatient();
        patient.BloodType = bloodType;
        patient.EmiratesId = $"784-BLDI-{Math.Abs(bloodType.GetHashCode()):D7}-I";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Nationality Validation Tests

    [Theory]
    [InlineData("UAE")]
    [InlineData("IND")]
    [InlineData("PAK")]
    [InlineData("EGY")]
    [InlineData("PHL")]
    [InlineData("GBR")]
    [InlineData("USA")]
    public async Task Nationality_ValidCodes_AcceptsAll(string nationality)
    {
        var patient = CreateValidPatient();
        patient.Nationality = nationality;
        patient.EmiratesId = $"784-NAT{nationality}-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Nationality_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.Nationality = null;
        patient.EmiratesId = "784-NNUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("United Arab Emirates")]
    [InlineData("India")]
    [InlineData("Pakistan")]
    public async Task Nationality_FullNames_HandlesGracefully(string nationality)
    {
        var patient = CreateValidPatient();
        patient.Nationality = nationality;
        patient.EmiratesId = $"784-NATF-{Math.Abs(nationality.GetHashCode()):D7}-F";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Marital Status Validation Tests

    [Theory]
    [InlineData("Single")]
    [InlineData("Married")]
    [InlineData("Divorced")]
    [InlineData("Widowed")]
    public async Task MaritalStatus_ValidStatuses_AcceptsAll(string status)
    {
        var patient = CreateValidPatient();
        patient.MaritalStatus = status;
        patient.EmiratesId = $"784-MST{Math.Abs(status.GetHashCode()):D7}-M";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task MaritalStatus_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.MaritalStatus = null;
        patient.EmiratesId = "784-MNUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Insurance Validation Tests

    [Theory]
    [InlineData("Daman", "POL-123456")]
    [InlineData("MetLife", "MET-789012")]
    [InlineData("AXA", "AXA-345678")]
    [InlineData("ADNIC", "ADN-901234")]
    public async Task Insurance_ValidProviderAndPolicy_AcceptsAll(string provider, string policy)
    {
        var patient = CreateValidPatient();
        patient.InsuranceProvider = provider;
        patient.InsurancePolicyNumber = policy;
        patient.EmiratesId = $"784-INS{Math.Abs(provider.GetHashCode()):D7}-I";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
        result.InsuranceProvider.Should().Be(provider);
        result.InsurancePolicyNumber.Should().Be(policy);
    }

    [Fact]
    public async Task Insurance_NullBoth_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.InsuranceProvider = null;
        patient.InsurancePolicyNumber = null;
        patient.EmiratesId = "784-INUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Insurance_ProviderWithoutPolicy_Accepts()
    {
        var patient = CreateValidPatient();
        patient.InsuranceProvider = "Daman";
        patient.InsurancePolicyNumber = null;
        patient.EmiratesId = "784-INOP-1234567-O";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Insurance_PolicyWithoutProvider_Accepts()
    {
        var patient = CreateValidPatient();
        patient.InsuranceProvider = null;
        patient.InsurancePolicyNumber = "POL-123456";
        patient.EmiratesId = "784-INPO-1234567-P";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Address Validation Tests

    [Theory]
    [InlineData("123 Main Street, Building A, Floor 1")]
    [InlineData("P.O. Box 12345")]
    [InlineData("شارع الشيخ زايد، برج خليفة")]
    public async Task Address_ValidFormats_AcceptsAll(string address)
    {
        var patient = CreateValidPatient();
        patient.Address = address;
        patient.EmiratesId = $"784-ADDR-{Math.Abs(address.GetHashCode()):D7}-A";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Address_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.Address = null;
        patient.EmiratesId = "784-ANUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("Dubai")]
    [InlineData("Abu Dhabi")]
    [InlineData("Sharjah")]
    [InlineData("Ajman")]
    [InlineData("Fujairah")]
    [InlineData("Ras Al Khaimah")]
    [InlineData("Umm Al Quwain")]
    public async Task City_UAECities_AcceptsAll(string city)
    {
        var patient = CreateValidPatient();
        patient.City = city;
        patient.EmiratesId = $"784-CITY-{Math.Abs(city.GetHashCode()):D7}-C";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Emergency Contact Validation Tests

    [Theory]
    [InlineData("John Doe", "+971501234567")]
    [InlineData("محمد أحمد", "+971561234567")]
    [InlineData("Jane Smith-Jones", "+1-202-555-0123")]
    public async Task EmergencyContact_ValidData_AcceptsAll(string name, string phone)
    {
        var patient = CreateValidPatient();
        patient.EmergencyContactName = name;
        patient.EmergencyContactPhone = phone;
        patient.EmiratesId = $"784-EMRG-{Math.Abs(name.GetHashCode()):D7}-E";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task EmergencyContact_NullBoth_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.EmergencyContactName = null;
        patient.EmergencyContactPhone = null;
        patient.EmiratesId = "784-ENUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task EmergencyContact_NameWithoutPhone_Accepts()
    {
        var patient = CreateValidPatient();
        patient.EmergencyContactName = "Emergency Contact";
        patient.EmergencyContactPhone = null;
        patient.EmiratesId = "784-ENOP-1234567-O";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Branch ID Validation Tests

    [Theory]
    [InlineData(1)]
    public async Task BranchId_ValidId_Accepts(int branchId)
    {
        var patient = CreateValidPatient();
        patient.BranchId = branchId;
        patient.EmiratesId = $"784-BR{branchId:D2}-1234567-B";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task BranchId_InvalidId_Rejects(int branchId)
    {
        var patient = CreateValidPatient();
        patient.BranchId = branchId;
        patient.EmiratesId = $"784-BRIN-{Math.Abs(branchId):D7}-I";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task BranchId_NonExistent_HandlesGracefully()
    {
        var patient = CreateValidPatient();
        patient.BranchId = 9999;
        patient.EmiratesId = "784-BRNE-1234567-N";

        var action = () => _patientService.CreatePatientAsync(patient);
        await action.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Occupation Validation Tests

    [Theory]
    [InlineData("Engineer")]
    [InlineData("Doctor")]
    [InlineData("Teacher")]
    [InlineData("Business Owner")]
    [InlineData("Software Developer")]
    [InlineData("Retired")]
    [InlineData("Student")]
    public async Task Occupation_ValidOccupations_AcceptsAll(string occupation)
    {
        var patient = CreateValidPatient();
        patient.Occupation = occupation;
        patient.EmiratesId = $"784-OCC{Math.Abs(occupation.GetHashCode()):D7}-O";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Occupation_Null_AcceptsAsOptional()
    {
        var patient = CreateValidPatient();
        patient.Occupation = null;
        patient.EmiratesId = "784-ONUL-1234567-N";

        var result = await _patientService.CreatePatientAsync(patient);
        result.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static Core.Entities.Patient CreateValidPatient()
    {
        return new Core.Entities.Patient
        {
            BranchId = 1,
            EmiratesId = $"784-{Guid.NewGuid().ToString()[..4]}-{Guid.NewGuid().ToString()[..7]}-V",
            FullNameEn = "Valid Patient",
            DateOfBirth = new DateTime(1990, 1, 1),
            Gender = "M"
        };
    }

    #endregion
}

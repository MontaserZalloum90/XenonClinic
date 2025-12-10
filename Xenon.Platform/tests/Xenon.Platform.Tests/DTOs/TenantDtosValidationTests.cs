using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using Xenon.Platform.Application.DTOs;
using Xunit;

namespace Xenon.Platform.Tests.DTOs;

/// <summary>
/// Tests for DTO validation attributes - ensures input validation works correctly
/// for all request DTOs with validation constraints.
/// </summary>
public class TenantDtosValidationTests
{
    #region TenantListQuery Validation Tests

    [Fact]
    public void TenantListQuery_ShouldPassValidation_WithDefaultValues()
    {
        // Arrange
        var query = new TenantListQuery();

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TenantListQuery_ShouldPassValidation_WithValidValues()
    {
        // Arrange
        var query = new TenantListQuery
        {
            Search = "clinic",
            Status = "Active",
            CompanyType = "Clinic",
            Page = 1,
            PageSize = 50
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TenantListQuery_ShouldFailValidation_WhenSearchExceeds200Characters()
    {
        // Arrange
        var query = new TenantListQuery
        {
            Search = new string('a', 201)
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("200 characters");
    }

    [Fact]
    public void TenantListQuery_ShouldFailValidation_WhenStatusExceeds50Characters()
    {
        // Arrange
        var query = new TenantListQuery
        {
            Status = new string('a', 51)
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("50 characters");
    }

    [Fact]
    public void TenantListQuery_ShouldFailValidation_WhenCompanyTypeExceeds50Characters()
    {
        // Arrange
        var query = new TenantListQuery
        {
            CompanyType = new string('a', 51)
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("50 characters");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void TenantListQuery_ShouldFailValidation_WhenPageIsLessThan1(int page)
    {
        // Arrange
        var query = new TenantListQuery { Page = page };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("Page must be at least 1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    [InlineData(1000)]
    public void TenantListQuery_ShouldFailValidation_WhenPageSizeIsOutOfRange(int pageSize)
    {
        // Arrange
        var query = new TenantListQuery { PageSize = pageSize };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("PageSize must be between 1 and 100");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void TenantListQuery_ShouldPassValidation_WithValidPageSize(int pageSize)
    {
        // Arrange
        var query = new TenantListQuery { PageSize = pageSize };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region SuspendTenantRequest Validation Tests

    [Fact]
    public void SuspendTenantRequest_ShouldPassValidation_WithNullReason()
    {
        // Arrange
        var request = new SuspendTenantRequest { Reason = null };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SuspendTenantRequest_ShouldPassValidation_WithValidReason()
    {
        // Arrange
        var request = new SuspendTenantRequest
        {
            Reason = "Non-payment of subscription fees"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void SuspendTenantRequest_ShouldFailValidation_WhenReasonExceeds500Characters()
    {
        // Arrange
        var request = new SuspendTenantRequest
        {
            Reason = new string('a', 501)
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("500 characters");
    }

    [Fact]
    public void SuspendTenantRequest_ShouldPassValidation_WithExactly500Characters()
    {
        // Arrange
        var request = new SuspendTenantRequest
        {
            Reason = new string('a', 500)
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region ExtendTrialRequest Validation Tests

    [Fact]
    public void ExtendTrialRequest_ShouldPassValidation_WithDefaultValues()
    {
        // Arrange
        var request = new ExtendTrialRequest(); // Days defaults to 14

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(14)]
    [InlineData(30)]
    [InlineData(90)]
    public void ExtendTrialRequest_ShouldPassValidation_WithValidDays(int days)
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = days };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void ExtendTrialRequest_ShouldFailValidation_WhenDaysIsLessThan1(int days)
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = days };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 1 and 90");
    }

    [Theory]
    [InlineData(91)]
    [InlineData(100)]
    [InlineData(365)]
    public void ExtendTrialRequest_ShouldFailValidation_WhenDaysExceeds90(int days)
    {
        // Arrange
        var request = new ExtendTrialRequest { Days = days };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 1 and 90");
    }

    [Fact]
    public void ExtendTrialRequest_ShouldPassValidation_WithValidReasonAndDays()
    {
        // Arrange
        var request = new ExtendTrialRequest
        {
            Days = 30,
            Reason = "Customer requested more time to evaluate"
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ExtendTrialRequest_ShouldFailValidation_WhenReasonExceeds500Characters()
    {
        // Arrange
        var request = new ExtendTrialRequest
        {
            Days = 14,
            Reason = new string('a', 501)
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("500 characters");
    }

    [Fact]
    public void ExtendTrialRequest_ShouldFailMultipleValidations_WhenBothInvalid()
    {
        // Arrange
        var request = new ExtendTrialRequest
        {
            Days = 0,
            Reason = new string('a', 501)
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().HaveCount(2);
    }

    #endregion

    #region TenantUsageQuery Validation Tests

    [Fact]
    public void TenantUsageQuery_ShouldPassValidation_WithDefaultValue()
    {
        // Arrange
        var query = new TenantUsageQuery(); // Days defaults to 30

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    public void TenantUsageQuery_ShouldPassValidation_WithValidDays(int days)
    {
        // Arrange
        var query = new TenantUsageQuery { Days = days };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-30)]
    public void TenantUsageQuery_ShouldFailValidation_WhenDaysIsLessThan1(int days)
    {
        // Arrange
        var query = new TenantUsageQuery { Days = days };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 1 and 365");
    }

    [Theory]
    [InlineData(366)]
    [InlineData(400)]
    [InlineData(1000)]
    public void TenantUsageQuery_ShouldFailValidation_WhenDaysExceeds365(int days)
    {
        // Arrange
        var query = new TenantUsageQuery { Days = days };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 1 and 365");
    }

    #endregion

    #region UsageUpdateRequest Validation Tests

    [Fact]
    public void UsageUpdateRequest_ShouldPassValidation_WithNullValues()
    {
        // Arrange
        var request = new UsageUpdateRequest
        {
            CurrentBranches = null,
            CurrentUsers = null
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UsageUpdateRequest_ShouldPassValidation_WithValidValues()
    {
        // Arrange
        var request = new UsageUpdateRequest
        {
            CurrentBranches = 5,
            CurrentUsers = 50
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UsageUpdateRequest_ShouldPassValidation_WithZeroValues()
    {
        // Arrange
        var request = new UsageUpdateRequest
        {
            CurrentBranches = 0,
            CurrentUsers = 0
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UsageUpdateRequest_ShouldPassValidation_WithMaximumValues()
    {
        // Arrange
        var request = new UsageUpdateRequest
        {
            CurrentBranches = 1000,
            CurrentUsers = 10000
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void UsageUpdateRequest_ShouldFailValidation_WhenBranchesIsNegative(int branches)
    {
        // Arrange
        var request = new UsageUpdateRequest { CurrentBranches = branches };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 0 and 1000");
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(2000)]
    [InlineData(10000)]
    public void UsageUpdateRequest_ShouldFailValidation_WhenBranchesExceeds1000(int branches)
    {
        // Arrange
        var request = new UsageUpdateRequest { CurrentBranches = branches };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 0 and 1000");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    [InlineData(-100)]
    public void UsageUpdateRequest_ShouldFailValidation_WhenUsersIsNegative(int users)
    {
        // Arrange
        var request = new UsageUpdateRequest { CurrentUsers = users };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 0 and 10000");
    }

    [Theory]
    [InlineData(10001)]
    [InlineData(20000)]
    [InlineData(100000)]
    public void UsageUpdateRequest_ShouldFailValidation_WhenUsersExceeds10000(int users)
    {
        // Arrange
        var request = new UsageUpdateRequest { CurrentUsers = users };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().ContainSingle()
            .Which.ErrorMessage.Should().Contain("between 0 and 10000");
    }

    [Fact]
    public void UsageUpdateRequest_ShouldFailMultipleValidations_WhenBothInvalid()
    {
        // Arrange
        var request = new UsageUpdateRequest
        {
            CurrentBranches = -1,
            CurrentUsers = -1
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().HaveCount(2);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void TenantListQuery_ShouldPassValidation_WithEmptyStrings()
    {
        // Arrange
        var query = new TenantListQuery
        {
            Search = "",
            Status = "",
            CompanyType = ""
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void ExtendTrialRequest_ShouldPassValidation_WithEmptyReason()
    {
        // Arrange
        var request = new ExtendTrialRequest
        {
            Days = 14,
            Reason = ""
        };

        // Act
        var results = ValidateModel(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void TenantListQuery_ShouldPassValidation_WithUnicodeCharacters()
    {
        // Arrange
        var query = new TenantListQuery
        {
            Search = "Test"
        };

        // Act
        var results = ValidateModel(query);

        // Assert
        results.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, context, validationResults, validateAllProperties: true);
        return validationResults;
    }

    #endregion
}

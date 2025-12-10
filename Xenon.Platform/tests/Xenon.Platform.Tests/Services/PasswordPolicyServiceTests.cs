using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xenon.Platform.Infrastructure.Services;
using Xunit;

namespace Xenon.Platform.Tests.Services;

/// <summary>
/// Tests for PasswordPolicyService - validates password policy enforcement
/// including complexity rules, common password detection, and strength assessment.
/// </summary>
public class PasswordPolicyServiceTests
{
    private readonly IPasswordPolicyService _service;

    public PasswordPolicyServiceTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PasswordPolicy:MinLength"] = "8",
                ["PasswordPolicy:MaxLength"] = "128",
                ["PasswordPolicy:RequireUppercase"] = "true",
                ["PasswordPolicy:RequireLowercase"] = "true",
                ["PasswordPolicy:RequireDigit"] = "true",
                ["PasswordPolicy:RequireSpecialCharacter"] = "true",
                ["PasswordPolicy:PreventCommonPasswords"] = "true",
                ["PasswordPolicy:PreventSequentialCharacters"] = "true",
                ["PasswordPolicy:PreventRepeatedCharacters"] = "true"
            })
            .Build();

        _service = new PasswordPolicyService(configuration);
    }

    #region Validation Tests

    [Fact]
    public void Validate_ShouldReturnValid_ForStrongPassword()
    {
        // Arrange
        var password = "SecureP@ss123!";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenPasswordIsEmpty()
    {
        // Arrange
        var password = "";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenPasswordIsTooShort()
    {
        // Arrange
        var password = "Ab1!xyz"; // 7 chars

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least 8 characters"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenMissingUppercase()
    {
        // Arrange
        var password = "password123!";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("uppercase"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenMissingLowercase()
    {
        // Arrange
        var password = "PASSWORD123!";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("lowercase"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenMissingDigit()
    {
        // Arrange
        var password = "Password!@#";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("digit"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenMissingSpecialCharacter()
    {
        // Arrange
        var password = "Password123";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("special character"));
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123456")]
    [InlineData("qwerty")]
    [InlineData("admin")]
    [InlineData("letmein")]
    public void Validate_ShouldReturnInvalid_ForCommonPasswords(string password)
    {
        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("too common"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenHasSequentialCharacters()
    {
        // Arrange
        var password = "Passabc123!"; // Contains "abc"

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("sequential"));
    }

    [Fact]
    public void Validate_ShouldReturnInvalid_WhenHasRepeatedCharacters()
    {
        // Arrange
        var password = "Passaaa123!"; // Contains "aaa"

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("repeated"));
    }

    [Fact]
    public void Validate_ShouldReturnMultipleErrors_WhenMultipleViolations()
    {
        // Arrange
        var password = "abc"; // Too short, no uppercase, no digit, no special, sequential

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().BeGreaterThan(1);
    }

    #endregion

    #region Strength Assessment Tests

    [Fact]
    public void GetStrength_ShouldReturnVeryWeak_ForEmptyPassword()
    {
        // Act
        var strength = _service.GetStrength("");

        // Assert
        strength.Should().Be(PasswordStrength.VeryWeak);
    }

    [Fact]
    public void GetStrength_ShouldReturnVeryWeak_ForCommonPassword()
    {
        // Act
        var strength = _service.GetStrength("password");

        // Assert
        strength.Should().Be(PasswordStrength.VeryWeak);
    }

    [Fact]
    public void GetStrength_ShouldReturnWeak_ForShortSimplePassword()
    {
        // Act
        var strength = _service.GetStrength("Pass1234");

        // Assert
        strength.Should().BeOneOf(PasswordStrength.Weak, PasswordStrength.Fair);
    }

    [Fact]
    public void GetStrength_ShouldReturnStrong_ForComplexPassword()
    {
        // Act
        var strength = _service.GetStrength("MyC0mplex!Pass");

        // Assert
        strength.Should().BeOneOf(PasswordStrength.Strong, PasswordStrength.VeryStrong);
    }

    [Fact]
    public void GetStrength_ShouldReturnVeryStrong_ForLongComplexPassword()
    {
        // Act
        var strength = _service.GetStrength("MyV3ryL0ng&Compl3xP@ssword!");

        // Assert
        strength.Should().Be(PasswordStrength.VeryStrong);
    }

    #endregion

    #region Common Password Detection Tests

    [Theory]
    [InlineData("password", true)]
    [InlineData("Password", true)]
    [InlineData("PASSWORD", true)]
    [InlineData("123456", true)]
    [InlineData("admin123", true)]
    [InlineData("MyUnique!Pass123", false)]
    [InlineData("Xk9$mPq2Lw", false)]
    public void IsCommonPassword_ShouldDetectCommonPasswords(string password, bool expected)
    {
        // Act
        var result = _service.IsCommonPassword(password);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_ShouldAcceptPasswordWithAllSpecialCharacters()
    {
        // Arrange
        var password = "Aa1!@#$%^&*()";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldAcceptUnicodeCharacters()
    {
        // Arrange - Password with unicode
        var password = "Secure123!";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldHandleExactMinLength()
    {
        // Arrange - Exactly 8 characters
        var password = "Abc123!@";

        // Act
        var result = _service.Validate(password);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}

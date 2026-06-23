using NuamExchange.Application.Security;

namespace NuamExchange.Api.Tests;

public sealed class PasswordPolicyTests
{
    private readonly DefaultPasswordPolicy policy = new();

    [Fact]
    public void IsValid_WithSecurePassword_ReturnsTrue() => Assert.True(policy.IsValid("PasswordSeguro!123"));

    [Fact]
    public void IsValid_WithoutUppercase_ReturnsFalse() => Assert.False(policy.IsValid("passwordseguro!123"));

    [Fact]
    public void IsValid_WithoutLowercase_ReturnsFalse() => Assert.False(policy.IsValid("PASSWORDSEGURO!123"));

    [Fact]
    public void IsValid_WithoutNumber_ReturnsFalse() => Assert.False(policy.IsValid("PasswordSeguro!!"));

    [Fact]
    public void IsValid_WithoutSymbol_ReturnsFalse() => Assert.False(policy.IsValid("PasswordSeguro123"));

    [Fact]
    public void IsValid_WithLessThanTwelveCharacters_ReturnsFalse() => Assert.False(policy.IsValid("Pass!12345"));
}

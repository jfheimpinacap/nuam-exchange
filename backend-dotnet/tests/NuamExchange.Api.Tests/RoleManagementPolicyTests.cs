using NuamExchange.Application.Administration;

namespace NuamExchange.Api.Tests;

public sealed class RoleManagementPolicyTests
{
    private readonly DefaultRoleManagementPolicy policy = new();

    [Fact]
    public void ValidateCustomRoleName_WithValidCustomRole_ReturnsNormalizedName()
    {
        var result = policy.ValidateCustomRoleName("  Revisor de Cargas  ");

        Assert.True(result.Succeeded);
        Assert.Equal("Revisor de Cargas", result.NormalizedName);
    }

    [Fact]
    public void ValidateCustomRoleName_WithEmptyName_ReturnsFalse()
        => Assert.False(policy.ValidateCustomRoleName("   ").Succeeded);

    [Fact]
    public void ValidateCustomRoleName_WithMoreThanEightyCharacters_ReturnsFalse()
        => Assert.False(policy.ValidateCustomRoleName(new string('A', 81)).Succeeded);

    [Theory]
    [InlineData("Administrador")]
    [InlineData("Analista Tributario")]
    [InlineData("Supervisor")]
    public void ValidateCustomRoleName_WithReservedName_ReturnsFalse(string roleName)
        => Assert.False(policy.ValidateCustomRoleName(roleName).Succeeded);

    [Theory]
    [InlineData("Administrador")]
    [InlineData(" administrador ")]
    [InlineData("Analista Tributario")]
    [InlineData("Supervisor")]
    public void IsSystemRole_WithBaseRole_ReturnsTrue(string roleName)
        => Assert.True(policy.IsSystemRole(roleName));

    [Fact]
    public void IsSystemRole_WithCustomRole_ReturnsFalse()
        => Assert.False(policy.IsSystemRole("Revisor de Cargas"));
}

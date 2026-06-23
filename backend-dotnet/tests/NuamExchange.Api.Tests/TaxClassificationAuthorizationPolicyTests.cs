using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NuamExchange.Infrastructure.DependencyInjection;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Api.Tests;

public sealed class TaxClassificationAuthorizationPolicyTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    [InlineData(SecuritySeedService.SupervisorRole)]
    public void TaxClassificationReadPolicy_AllowsDefinedRole(string roleName)
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(new ConfigurationBuilder().Build());
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;

        var policy = options.GetPolicy("TaxClassificationRead");

        Assert.NotNull(policy);
        var rolesRequirement = Assert.Single(policy!.Requirements.OfType<RolesAuthorizationRequirement>());
        Assert.Contains(roleName, rolesRequirement.AllowedRoles);
    }
}

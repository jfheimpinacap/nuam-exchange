using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NuamExchange.Api.Contracts.Setup;
using NuamExchange.Infrastructure.Persistence;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class ProductionBootstrapTests
{
    private const string BootstrapKey = "TEST_BOOTSTRAP_KEY_WITH_AT_LEAST_32_CHARS";

    [Fact]
    public async Task BootstrapProduction_WhenDisabled_ReturnsNotFound()
    {
        using var factory = CreateFactory(bootstrapEnabled: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/setup/bootstrap-production", ValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BootstrapProduction_WithWrongKey_ReturnsNotFound()
    {
        using var factory = CreateFactory(bootstrapEnabled: true);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Nuam-Bootstrap-Key", "WRONG_BOOTSTRAP_KEY_WITH_AT_LEAST_32_CHARS");

        using var response = await client.PostAsJsonAsync("/api/setup/bootstrap-production", ValidRequest());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task BootstrapProduction_WithValidKey_CreatesAdministratorWhenNoUsersExist()
    {
        using var factory = CreateFactory(bootstrapEnabled: true);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Nuam-Bootstrap-Key", BootstrapKey);

        using var response = await client.PostAsJsonAsync("/api/setup/bootstrap-production", ValidRequest());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>();
        var user = Assert.Single(dbContext.Users);
        Assert.Equal("admin@example.com", user.Email);
        Assert.NotEqual("AdminPass123!", user.PasswordHash);
    }

    [Fact]
    public async Task BootstrapProduction_WhenUserAlreadyExists_ReturnsConflict()
    {
        using var factory = CreateFactory(bootstrapEnabled: true);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Nuam-Bootstrap-Key", BootstrapKey);

        using var firstResponse = await client.PostAsJsonAsync("/api/setup/bootstrap-production", ValidRequest());
        using var secondResponse = await client.PostAsJsonAsync("/api/setup/bootstrap-production", ValidRequest(email: "second@example.com"));

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>();
        Assert.Single(dbContext.Users);
    }

    private static BootstrapAdminRequest ValidRequest(string email = "admin@example.com") => new()
    {
        FullName = "Initial Administrator",
        Email = email,
        Password = "AdminPass123!",
        JobTitle = "Administrator"
    };

    private static WebApplicationFactory<Program> CreateFactory(bool bootstrapEnabled)
    {
        var databaseName = TestInMemoryDatabase.CreateDatabaseName();
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Production");
            builder.ConfigureAppConfiguration((_, configurationBuilder) =>
            {
                configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:NuamTributariaDb"] = "Server=(local);Database=NuamExchangeTests;Trusted_Connection=True;TrustServerCertificate=True",
                    ["Jwt:SigningKey"] = "TEST_SIGNING_KEY_WITH_AT_LEAST_32_CHARACTERS_123",
                    ["Bootstrap:Enabled"] = bootstrapEnabled.ToString(),
                    ["Bootstrap:Key"] = BootstrapKey
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddNuamExchangeInMemoryDatabase(databaseName);
            });
        });
    }
}

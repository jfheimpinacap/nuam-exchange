using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuamExchange.Application.TaxClassifications;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class TaxClassificationsEndpointBindingTests
{
    [Fact]
    public async Task PostTaxClassifications_WithPublishedJsonContract_DoesNotFailModelBinding()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ITaxClassificationCommandService>();
                services.AddScoped<ITaxClassificationCommandService, CapturingTaxClassificationCommandService>();
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme;
                    options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, _ => { });
            });
        });
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/tax-classifications", new
        {
            market = "BOLSA",
            instrumentCode = "NUEX-PRUEBA-20260624014603090",
            instrumentName = "Instrumento de Prueba Nuam",
            classificationType = "DIVIDENDO",
            description = "Calificación inicial creada para validar el flujo local.",
            updatePercentage = 0m,
            appliedFactor = 1m,
            referenceAmount = 100000m,
            currency = "CLP",
            taxPeriod = 2026,
            validFrom = new DateOnly(2026, 1, 1),
            validTo = new DateOnly(2026, 12, 31)
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var created = await response.Content.ReadFromJsonAsync<TaxClassificationDetailDto>();
        Assert.NotNull(created);
        Assert.Equal("Calificación inicial creada para validar el flujo local.", created.Description);
    }

    private sealed class CapturingTaxClassificationCommandService : ITaxClassificationCommandService
    {
        public Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default)
        {
            var response = new TaxClassificationDetailDto(
                101,
                command.CreatorUserId,
                command.Market,
                command.InstrumentCode,
                command.InstrumentName,
                command.ClassificationType,
                command.Description,
                command.UpdatePercentage,
                command.AppliedFactor,
                command.ReferenceAmount,
                command.Currency,
                command.TaxPeriod,
                command.ValidFrom,
                command.ValidTo,
                "Borrador",
                DateTime.UtcNow,
                DateTime.UtcNow);
            return Task.FromResult(response);
        }

        public Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default)
        {
            var response = new TaxClassificationDetailDto(
                202,
                command.ActorUserId,
                "BOLSA",
                "COPIA",
                "Instrumento Copia",
                "DIVIDENDO",
                "Copia",
                0m,
                1m,
                100m,
                "CLP",
                2026,
                new DateOnly(2026, 1, 1),
                null,
                "VIGENTE",
                DateTime.UtcNow,
                DateTime.UtcNow);
            return Task.FromResult<TaxClassificationDetailDto?>(response);
        }

        public Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default)
        {
            var response = new TaxClassificationDetailDto(
                command.Id,
                1,
                command.Market,
                command.InstrumentCode,
                command.InstrumentName,
                command.ClassificationType,
                command.Description,
                command.UpdatePercentage,
                command.AppliedFactor,
                command.ReferenceAmount,
                command.Currency,
                command.TaxPeriod,
                command.ValidFrom,
                command.ValidTo,
                "VIGENTE",
                new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateTime.UtcNow);
            return Task.FromResult<TaxClassificationDetailDto?>(command.Id == 404 ? null : response);
        }
    }

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "Test";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { new Claim("sub", "1"), new Claim(ClaimTypes.Role, "Administrador") };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}

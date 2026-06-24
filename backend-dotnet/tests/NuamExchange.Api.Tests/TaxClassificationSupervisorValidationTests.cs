using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;
using NuamExchange.Infrastructure.TaxClassifications;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class TaxClassificationSupervisorValidationTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.SupervisorRole)]
    public async Task SupervisorValidationEndpoint_WithSupervisoryRole_ReturnsOk(string role)
    {
        using var factory = CreateFactory(role, authenticated: true);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/tax-classifications/7/supervisor-validation", new { decision = "OBSERVADO", observation = "Revisión supervisora" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaxClassificationDetailDto>();
        Assert.NotNull(body);
        Assert.Equal("OBSERVADA", body!.Status);
    }

    [Fact]
    public async Task SupervisorValidationEndpoint_WithTaxAnalyst_ReturnsForbidden()
    {
        using var factory = CreateFactory(SecuritySeedService.TaxAnalystRole, authenticated: true);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/tax-classifications/7/supervisor-validation", new { decision = "OBSERVADO" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SupervisorValidationEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole, authenticated: false);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/api/tax-classifications/7/supervisor-validation", new { decision = "OBSERVADO" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData(404, "OBSERVADO", HttpStatusCode.NotFound)]
    [InlineData(7, "", HttpStatusCode.BadRequest)]
    [InlineData(7, "RECHAZADO", HttpStatusCode.BadRequest)]
    [InlineData(9, "OBSERVADO", HttpStatusCode.Conflict)]
    public async Task SupervisorValidationEndpoint_MapsExpectedErrors(int id, string decision, HttpStatusCode expectedStatus)
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole, authenticated: true);
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync($"/api/tax-classifications/{id}/supervisor-validation", new { decision });

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    [Fact]
    public async Task SupervisorValidationService_CreatesValidationHistoryAuditAndUpdatesOnlyStatusAndUpdatedAt()
    {
        var options = new DbContextOptionsBuilder<NuamExchangeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var createdAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using var dbContext = new NuamExchangeDbContext(options);
        dbContext.TaxClassifications.Add(new TaxClassification
        {
            Id = 7,
            CreatorUserId = 10,
            Market = "BOLSA",
            InstrumentCode = "NUAM",
            InstrumentName = "Instrumento",
            ClassificationType = "DIVIDENDO",
            Description = "Descripción",
            UpdatePercentage = 1m,
            AppliedFactor = 2m,
            ReferenceAmount = 100m,
            Currency = "CLP",
            TaxPeriod = 2026,
            ValidFrom = new DateOnly(2026, 1, 1),
            ValidTo = null,
            Status = "VIGENTE",
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        });
        await dbContext.SaveChangesAsync();

        var service = new TaxClassificationCommandService(dbContext);
        var result = await service.SupervisorValidationAsync(new ValidatedSupervisorValidationTaxClassificationCommand(7, 99, "OBSERVADO", "Observación real", "127.0.0.1"));

        Assert.True(result.Succeeded);
        Assert.Equal("OBSERVADA", result.Value!.Status);
        Assert.Equal(10, result.Value.CreatorUserId);
        Assert.Equal(createdAt, result.Value.CreatedAt);
        Assert.True(result.Value.UpdatedAt > createdAt);
        Assert.Equal("BOLSA", result.Value.Market);
        Assert.Equal("NUAM", result.Value.InstrumentCode);

        var validation = await dbContext.TaxValidations.SingleAsync();
        Assert.Equal(7, validation.TaxClassificationId);
        Assert.Equal(99, validation.UserId);
        Assert.Equal("OBSERVADO", validation.Result);
        Assert.Equal("Observación real", validation.Observation);

        var history = await dbContext.ClassificationHistories.SingleAsync();
        Assert.Equal("OBSERVACION", history.ChangeType);
        Assert.Equal("Status", history.ModifiedField);
        Assert.Equal("VIGENTE", history.PreviousValue);
        Assert.Equal("OBSERVADA", history.NewValue);

        var audit = await dbContext.AuditLogs.SingleAsync();
        Assert.Equal("TAX_CLASSIFICATION_VALIDATED", audit.Action);
        Assert.Equal("CalificacionTributaria", audit.AffectedEntity);
        Assert.Equal(7, audit.AffectedRecordId);
    }

    [Fact]
    public async Task SupervisorValidationService_InvalidTransition_DoesNotCreatePartialData()
    {
        var options = new DbContextOptionsBuilder<NuamExchangeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var dbContext = new NuamExchangeDbContext(options);
        dbContext.TaxClassifications.Add(new TaxClassification { Id = 9, CreatorUserId = 10, Market = "BOLSA", ClassificationType = "DIVIDENDO", Currency = "CLP", TaxPeriod = 2026, ValidFrom = new DateOnly(2026, 1, 1), Status = "ANULADA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        await dbContext.SaveChangesAsync();

        var service = new TaxClassificationCommandService(dbContext);
        var result = await service.SupervisorValidationAsync(new ValidatedSupervisorValidationTaxClassificationCommand(9, 99, "OBSERVADO", null, null));

        Assert.False(result.Succeeded);
        Assert.Equal(409, result.StatusCode);
        Assert.Empty(dbContext.TaxValidations);
        Assert.Empty(dbContext.ClassificationHistories);
        Assert.Empty(dbContext.AuditLogs);
        Assert.Equal("ANULADA", (await dbContext.TaxClassifications.SingleAsync()).Status);
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaxClassificationCommandService>();
            services.AddScoped<ITaxClassificationCommandService, FakeSupervisorValidationCommandService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RoleAuthenticationHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = RoleAuthenticationHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, RoleAuthenticationHandler>(RoleAuthenticationHandler.AuthenticationScheme, options => { options.ClaimsIssuer = authenticated ? role : ""; });
        });
    });

    private sealed class FakeSupervisorValidationCommandService : ITaxClassificationCommandService
    {
        public Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<BulkLoadXFactorResult> BulkLoadXFactorAsync(BulkLoadXFactorCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<BulkLoadXAmountResult> BulkLoadXAmountAsync(BulkLoadXAmountCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<SupervisorValidationResult> SupervisorValidationAsync(ValidatedSupervisorValidationTaxClassificationCommand command, CancellationToken cancellationToken = default)
        {
            if (command.Id == 404) return Task.FromResult(SupervisorValidationResult.Failure(404, "La calificación tributaria no existe."));
            if (command.Id == 9) return Task.FromResult(SupervisorValidationResult.Failure(409, "La decisión no está permitida desde el estado actual de la calificación tributaria."));
            return Task.FromResult(SupervisorValidationResult.Success(new TaxClassificationDetailDto(command.Id, 10, "BOLSA", "NUAM", "Instrumento", "DIVIDENDO", "Validada", 0m, 1m, 100m, "CLP", 2026, new DateOnly(2026, 1, 1), null, "OBSERVADA", DateTime.UtcNow, DateTime.UtcNow)));
        }
    }

    private sealed class RoleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "SupervisorRoleTest";
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Options.ClaimsIssuer;
            if (string.IsNullOrWhiteSpace(role)) return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new[] { new Claim("sub", "99"), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}

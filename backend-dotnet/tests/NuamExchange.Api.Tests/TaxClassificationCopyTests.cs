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

public sealed class TaxClassificationCopyTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    public async Task CopyEndpoint_WithWriteRole_ReturnsCreatedWithLocation(string role)
    {
        using var factory = CreateFactory(role);
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/tax-classifications/7/copy", content: null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
        var copied = await response.Content.ReadFromJsonAsync<TaxClassificationDetailDto>();
        Assert.NotNull(copied);
        Assert.NotEqual(7, copied!.Id);
    }

    [Fact]
    public async Task CopyEndpoint_WithMissingSource_ReturnsNotFound()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/tax-classifications/404/copy", content: null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CopyEndpoint_WithSupervisor_ReturnsForbidden()
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole);
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/tax-classifications/7/copy", content: null);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CopyService_CopiesEditableFieldsAndCreatesIndependentHistoryAndAudit()
    {
        var options = new DbContextOptionsBuilder<NuamExchangeDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var dbContext = new NuamExchangeDbContext(options);
        var source = new TaxClassification
        {
            Id = 7,
            CreatorUserId = 10,
            Market = "BOLSA",
            InstrumentCode = "NUAM-ORIGEN",
            InstrumentName = "Instrumento Origen",
            ClassificationType = "DIVIDENDO",
            Description = "Descripción origen",
            UpdatePercentage = 1.25m,
            AppliedFactor = 2.5m,
            ReferenceAmount = 1000.75m,
            Currency = "CLP",
            TaxPeriod = 2026,
            ValidFrom = new DateOnly(2026, 1, 1),
            ValidTo = new DateOnly(2026, 12, 31),
            Status = "OBSERVADA",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        dbContext.TaxClassifications.Add(source);
        dbContext.ClassificationHistories.Add(new ClassificationHistory { Id = 1, TaxClassificationId = source.Id, UserId = 10, ChangeType = "MODIFICACION", Observation = "Historial previo", ChangedAt = source.UpdatedAt });
        await dbContext.SaveChangesAsync();

        var service = new TaxClassificationCommandService(dbContext);
        var copied = await service.CopyAsync(new CopyTaxClassificationCommand(source.Id, 99, "127.0.0.1"));

        Assert.NotNull(copied);
        Assert.NotEqual(source.Id, copied!.Id);
        Assert.Equal(99, copied.CreatorUserId);
        Assert.Equal("VIGENTE", copied.Status);
        Assert.True(copied.CreatedAt > source.CreatedAt);
        Assert.Equal(copied.CreatedAt, copied.UpdatedAt);
        Assert.Equal(source.Market, copied.Market);
        Assert.Equal(source.InstrumentCode, copied.InstrumentCode);
        Assert.Equal(source.InstrumentName, copied.InstrumentName);
        Assert.Equal(source.ClassificationType, copied.ClassificationType);
        Assert.Equal(source.Description, copied.Description);
        Assert.Equal(source.UpdatePercentage, copied.UpdatePercentage);
        Assert.Equal(source.AppliedFactor, copied.AppliedFactor);
        Assert.Equal(source.ReferenceAmount, copied.ReferenceAmount);
        Assert.Equal(source.Currency, copied.Currency);
        Assert.Equal(source.TaxPeriod, copied.TaxPeriod);
        Assert.Equal(source.ValidFrom, copied.ValidFrom);
        Assert.Equal(source.ValidTo, copied.ValidTo);

        var reloadedSource = await dbContext.TaxClassifications.AsNoTracking().SingleAsync(x => x.Id == source.Id);
        Assert.Equal("OBSERVADA", reloadedSource.Status);
        Assert.Equal(10, reloadedSource.CreatorUserId);
        Assert.Equal(source.CreatedAt, reloadedSource.CreatedAt);

        var copiedHistory = await dbContext.ClassificationHistories.SingleAsync(x => x.TaxClassificationId == copied.Id);
        Assert.Equal("CREACION", copiedHistory.ChangeType);
        Assert.Equal(99, copiedHistory.UserId);
        Assert.Contains(source.Id.ToString(), copiedHistory.Observation);
        Assert.Single(dbContext.ClassificationHistories.Where(x => x.TaxClassificationId == source.Id));

        var audit = await dbContext.AuditLogs.SingleAsync(x => x.AffectedRecordId == copied.Id);
        Assert.Equal("TAX_CLASSIFICATION_COPIED", audit.Action);
        Assert.Equal(99, audit.UserId);
        Assert.Equal("CalificacionTributaria", audit.AffectedEntity);
    }

    private static WebApplicationFactory<Program> CreateFactory(string role) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaxClassificationCommandService>();
            services.AddScoped<ITaxClassificationCommandService, FakeCopyCommandService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RoleAuthenticationHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = RoleAuthenticationHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, RoleAuthenticationHandler>(RoleAuthenticationHandler.AuthenticationScheme, options => { options.ClaimsIssuer = role; });
        });
    });

    private sealed class FakeCopyCommandService : ITaxClassificationCommandService
    {
        public Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<SupervisorValidationResult> SupervisorValidationAsync(ValidatedSupervisorValidationTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default)
        {
            if (command.SourceId == 404) return Task.FromResult<TaxClassificationDetailDto?>(null);
            return Task.FromResult<TaxClassificationDetailDto?>(new TaxClassificationDetailDto(108, command.ActorUserId, "BOLSA", "NUAM", "Instrumento", "DIVIDENDO", "Copia", 0m, 1m, 100m, "CLP", 2026, new DateOnly(2026, 1, 1), null, "VIGENTE", DateTime.UtcNow, DateTime.UtcNow));
        }
    }

    private sealed class RoleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "RoleTest";
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Options.ClaimsIssuer ?? SecuritySeedService.AdministratorRole;
            var claims = new[] { new Claim("sub", "99"), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}

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
using NuamExchange.Application.TaxReports;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.TaxReports;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Api.Tests;

public sealed class TaxReportQueryTests
{

    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    [InlineData(SecuritySeedService.SupervisorRole)]
    public async Task Endpoints_WithReadRoles_ReturnOk(string role)
    {
        using var factory = CreateFactory(role);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/tax-reports/tax-classifications")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/tax-reports/tax-classifications/export")).StatusCode);
    }

    [Fact]
    public async Task Endpoints_WithoutJwt_ReturnUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, authenticated: false);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/tax-reports/tax-classifications")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/tax-reports/tax-classifications/export")).StatusCode);
    }

    [Fact]
    public async Task JsonEndpoint_ReturnsSafeContractAndCsvHeaders()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/tax-reports/tax-classifications?market=BOLSA&page=1&pageSize=1&sortBy=taxPeriod&sortDirection=desc");
        var body = await response.Content.ReadAsStringAsync();
        var report = await response.Content.ReadFromJsonAsync<TaxClassificationReportDto>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(report); Assert.NotNull(report!.Items); Assert.Equal(3, report.TotalCount); Assert.Equal(3, report.TotalPages);
        Assert.DoesNotContain("filePath", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claims", body, StringComparison.OrdinalIgnoreCase);
        var csv = await client.GetAsync("/api/tax-reports/tax-classifications/export?market=BOLSA&sortBy=id&sortDirection=asc");
        Assert.Equal(HttpStatusCode.OK, csv.StatusCode);
        Assert.Contains("text/csv", csv.Content.Headers.ContentType?.ToString());
        Assert.Contains("reporte_calificaciones_tributarias_", csv.Content.Headers.ContentDisposition?.FileName ?? string.Empty);
    }
    [Theory]
    [InlineData(0, 20, "taxPeriod", "desc")]
    [InlineData(1, 0, "taxPeriod", "desc")]
    [InlineData(1, 101, "taxPeriod", "desc")]
    [InlineData(1, 20, "filePath", "desc")]
    [InlineData(1, 20, "taxPeriod", "sideways")]
    public void Validator_RejectsUnsafeParameters(int page, int pageSize, string sortBy, string sortDirection)
    {
        var result = new TaxReportQueryValidator().Validate(new TaxClassificationReportQuery(null, null, null, null, null, null, page, pageSize, sortBy, sortDirection));
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Report_AppliesFiltersPaginationSummaryAndDoesNotModifyData()
    {
        await using var db = CreateDb(); Seed(db); await db.SaveChangesAsync();
        var service = new TaxReportQueryService(db);
        var report = await service.GetTaxClassificationsAsync(new ValidatedTaxClassificationReportQuery("BOLSA", null, 2026, null, null, null, 1, 1, "taxPeriod", "desc"));
        Assert.NotNull(report.Items); Assert.Single(report.Items); Assert.Equal(3, report.TotalCount); Assert.Equal(3, report.TotalPages);
        Assert.Equal(3, report.Summary.TotalClassifications); Assert.Equal(2, report.Summary.CountWithReferenceAmount); Assert.Equal(2, report.Summary.ReferenceAmountTotalsByCurrency.Count);
        Assert.Contains(report.Summary.ReferenceAmountTotalsByCurrency, x => x.Currency == "CLP" && x.TotalReferenceAmount == 100m);
        Assert.Contains(report.Summary.ReferenceAmountTotalsByCurrency, x => x.Currency == "USD" && x.TotalReferenceAmount == 20m);
        Assert.DoesNotContain(db.ChangeTracker.Entries(), e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted);
    }

    [Fact]
    public async Task Csv_UsesBomSemicolonEscapingFormulaProtectionAndLimit()
    {
        await using var db = CreateDb(); Seed(db); await db.SaveChangesAsync();
        var service = new TaxReportQueryService(db);
        var csv = await service.ExportTaxClassificationsAsync(new ValidatedTaxClassificationReportQuery("BOLSA", null, 2026, null, null, null, 1, 20, "id", "asc"));
        var text = System.Text.Encoding.UTF8.GetString(csv.Content);
        Assert.StartsWith("\uFEFFmarket;instrumentCode;instrumentName;classificationType;taxPeriod;status;", text);
        Assert.Contains("'=CMD", text); Assert.Contains("\"Name; \"\"quoted\"\"", text); Assert.DoesNotContain("FilePath", text);

        for (var i = 10; i < TaxClassificationReportDefaults.MaxExportRows + 20; i++) db.TaxClassifications.Add(NewTax(i, "BOLSA", "CLP", 2026));
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ExportTaxClassificationsAsync(new ValidatedTaxClassificationReportQuery(null, null, null, null, null, null, 1, 20, "id", "asc")));
    }

    private static NuamExchangeDbContext CreateDb() => new(new DbContextOptionsBuilder<NuamExchangeDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options);
    private static TaxClassification NewTax(int id, string market, string currency, int period) => new() { Id = id, CreatorUserId = 1, Market = market, InstrumentCode = "INS" + id, InstrumentName = "Instrument " + id, ClassificationType = "DIVIDENDO", Currency = currency, TaxPeriod = period, ValidFrom = new DateOnly(period, 1, 1), Status = "VIGENTE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
    private static TaxClassification CreateTax(int id, string market, string currency, int period, string? code, string? name, decimal? amount, decimal? factor) { var x = NewTax(id, market, currency, period); x.InstrumentCode = code ?? x.InstrumentCode; x.InstrumentName = name ?? x.InstrumentName; x.ReferenceAmount = amount; x.AppliedFactor = factor; return x; }
    private static void Seed(NuamExchangeDbContext db)
    {
        db.Roles.Add(new Role { Id = 1, Name = "Administrador", CreatedAt = DateTime.UtcNow });
        db.Users.Add(new ApplicationUser { Id = 1, FullName = "User", Email = "u@example.test", PasswordHash = "hash", RoleId = 1, CreatedAt = DateTime.UtcNow });
        db.TaxClassifications.AddRange(
            CreateTax(1, "BOLSA", "CLP", 2026, "=CMD", "Name; \"quoted\"\nline", 100m, 1.1m),
            CreateTax(2, "BOLSA", "USD", 2026, null, null, 20m, null),
            NewTax(3, "BOLSA", "CLP", 2026),
            CreateTax(4, "OTC", "CLP", 2025, null, null, 999m, null));
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<NuamExchangeDbContext>(); services.RemoveAll<DbContextOptions<NuamExchangeDbContext>>();
            services.AddDbContext<NuamExchangeDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            services.RemoveAll<ITaxReportQueryService>(); services.AddScoped<ITaxReportQueryService, TaxReportQueryService>();
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme; options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme; }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, o => { o.ClaimsIssuer = authenticated ? role : "__unauthenticated__"; });
            using var sp = services.BuildServiceProvider(); using var scope = sp.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); Seed(db); db.SaveChanges();
        });
    });

    private sealed class TestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "Test";
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Options.ClaimsIssuer == "__unauthenticated__") return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity(new[] { new Claim("sub", "1"), new Claim(ClaimTypes.Role, Options.ClaimsIssuer!) }, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}

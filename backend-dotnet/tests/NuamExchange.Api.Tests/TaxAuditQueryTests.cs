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
using NuamExchange.Application.TaxAudits;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;
using NuamExchange.Infrastructure.TaxAudits;

namespace NuamExchange.Api.Tests;

public sealed class TaxAuditQueryTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    [InlineData(SecuritySeedService.SupervisorRole)]
    public async Task ReadRoles_CanQueryTaxAudits(string role)
    {
        using var factory = CreateFactory(role);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/tax-audits")).StatusCode);
    }

    [Fact]
    public async Task WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, authenticated: false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/tax-audits")).StatusCode);
    }

    [Fact]
    public async Task List_FiltersPaginatesSortsAndExcludesNonTaxAudits()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        await AssertSeedVisible(factory.Services);
        var response = await client.GetAsync("/api/tax-audits?page=1&pageSize=2&sortBy=occurredAt&sortDirection=desc");
        var body = await response.Content.ReadAsStringAsync();
        var page = await response.Content.ReadFromJsonAsync<PagedResult<TaxAuditListItemDto>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(page?.Items);
        Assert.Equal(6, page!.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(2, page.Items.Count);
        Assert.DoesNotContain("USER_CREATED", body);
        Assert.DoesNotContain("Authentication", body);
        Assert.DoesNotContain("ipAddress", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("originIp", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("email", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("claim", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("connection string", body, StringComparison.OrdinalIgnoreCase);

        var byAction = await client.GetFromJsonAsync<PagedResult<TaxAuditListItemDto>>("/api/tax-audits?action=TAX_CLASSIFICATION_UPDATED");
        Assert.Single(byAction!.Items);
        var byTax = await client.GetFromJsonAsync<PagedResult<TaxAuditListItemDto>>("/api/tax-audits?taxClassificationId=10");
        Assert.Equal(2, byTax!.TotalCount);
        var byDate = await client.GetFromJsonAsync<PagedResult<TaxAuditListItemDto>>("/api/tax-audits?dateFrom=2026-06-02T00:00:00Z&dateTo=2026-06-03T23:59:59Z&sortBy=id&sortDirection=asc");
        Assert.Equal(2, byDate!.TotalCount);
    }

    [Fact]
    public void TaxAuditScope_CanGenerateRelationalSqlWithoutConnectingToDatabase()
    {
        var options = new DbContextOptionsBuilder<NuamExchangeDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=NuamExchangeSqlTranslationOnly;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var db = new NuamExchangeDbContext(options);

        var sql = TaxAuditQueryService.ApplyTaxAuditScope(db.AuditLogs.AsNoTracking()).ToQueryString();

        Assert.Contains("[a].[entidad_afectada] = N'CalificacionTributaria'", sql);
        Assert.Contains("[a].[registro_afectado_id] IS NOT NULL", sql);
        Assert.Contains("[a].[accion]", sql);
        Assert.Contains("TAX_CLASSIFICATION_CREATED", sql);
        Assert.Contains("TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED", sql);
    }

    [Fact]
    public async Task EmptyList_ReturnsNonNullItemsAndZeroPages()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, seedTaxAudits: false);
        var page = await factory.CreateClient().GetFromJsonAsync<PagedResult<TaxAuditListItemDto>>("/api/tax-audits");
        Assert.NotNull(page?.Items);
        Assert.Empty(page!.Items);
        Assert.Equal(0, page.TotalCount);
        Assert.Equal(0, page.TotalPages);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("sortBy=originIp")]
    [InlineData("sortDirection=sideways")]
    [InlineData("dateFrom=2026-06-05T00:00:00Z&dateTo=2026-06-01T00:00:00Z")]
    [InlineData("action=USER_CREATED")]
    public async Task InvalidParameters_ReturnBadRequest(string query)
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        Assert.Equal(HttpStatusCode.BadRequest, (await factory.CreateClient().GetAsync($"/api/tax-audits?{query}")).StatusCode);
    }

    [Fact]
    public async Task Detail_ExistingTaxAuditReturnsSafeDto_AndNonTaxOrMissingReturn404()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        var detailResponse = await client.GetAsync("/api/tax-audits/1");
        var body = await detailResponse.Content.ReadAsStringAsync();
        var detail = await detailResponse.Content.ReadFromJsonAsync<TaxAuditDetailDto>();
        Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
        Assert.Equal("TAX_CLASSIFICATION_CREATED", detail!.Action);
        Assert.Equal(10, detail.TaxClassificationId);
        Assert.DoesNotContain("127.0.0.1", body);
        Assert.DoesNotContain("password", body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/tax-audits/100")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/tax-audits/9999")).StatusCode);
    }

    [Fact]
    public async Task Query_DoesNotModifyOperationalData()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var before = await SnapshotAsync(factory.Services);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/tax-audits")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/tax-audits/1")).StatusCode);
        Assert.Equal(before, await SnapshotAsync(factory.Services));
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true, bool seedTaxAudits = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
    {
        services.RemoveAll<NuamExchangeDbContext>(); services.RemoveAll<DbContextOptions<NuamExchangeDbContext>>();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<NuamExchangeDbContext>(o => o.UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.RemoveAll<ITaxAuditQueryService>(); services.AddScoped<ITaxAuditQueryService, TaxAuditQueryService>();
        services.AddAuthentication(options => { options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme; options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme; }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, o => { o.ClaimsIssuer = authenticated ? role : "__unauthenticated__"; });
        using var sp = services.BuildServiceProvider(); using var scope = sp.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); Seed(db, seedTaxAudits); db.SaveChanges();
    }));

    private static void Seed(NuamExchangeDbContext db, bool seedTaxAudits)
    {
        db.Roles.Add(new Role { Id = 1, Name = "Administrador", CreatedAt = DateTime.UtcNow });
        db.Users.Add(new ApplicationUser { Id = 1, FullName = "User", Email = "u@example.test", PasswordHash = "hash", RoleId = 1, CreatedAt = DateTime.UtcNow });
        db.TaxClassifications.Add(new TaxClassification { Id = 10, CreatorUserId = 1, Market = "BOLSA", ClassificationType = "DIVIDENDO", Currency = "CLP", TaxPeriod = 2026, ValidFrom = new DateOnly(2026, 1, 1), Status = "VIGENTE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        if (!seedTaxAudits) return;
        var dates = Enumerable.Range(1, 6).Select(d => new DateTime(2026, 6, d, 12, 0, 0, DateTimeKind.Utc)).ToArray();
        db.AuditLogs.AddRange(
            TaxAudit(1, 10, "TAX_CLASSIFICATION_CREATED", dates[0], "Creada", null, null), TaxAudit(2, 10, "TAX_CLASSIFICATION_UPDATED", dates[1], "Actualizada", null, null), TaxAudit(3, 11, "TAX_CLASSIFICATION_COPIED", dates[2], "Copiada", null, null), TaxAudit(4, 12, "TAX_CLASSIFICATION_VALIDATED", dates[3], "Validada", "VIGENTE", "OBSERVADA"), TaxAudit(5, 13, "TAX_CLASSIFICATION_FACTOR_BULK_UPDATED", dates[4], "Factor", "1", "2"), TaxAudit(6, 14, "TAX_CLASSIFICATION_AMOUNT_BULK_UPDATED", dates[5], "Monto", "10", "20"),
            new AuditLog { Id = 100, UserId = 1, AffectedEntity = "Usuario", AffectedRecordId = 1, Action = "USER_CREATED", Detail = "email u@example.test password hash", OriginIp = "127.0.0.1", ActionAt = dates[0] },
            new AuditLog { Id = 101, UserId = 1, AffectedEntity = "Authentication", Action = "LOGIN_SUCCESS", Detail = "JWT claim", OriginIp = "127.0.0.1", ActionAt = dates[0] });
    }

    private static AuditLog TaxAudit(int id, int recordId, string action, DateTime date, string detail, string? previous, string? newer) => new() { Id = id, UserId = 1, AffectedEntity = TaxAuditRules.TaxClassificationEntity, AffectedRecordId = recordId, Action = action, Detail = detail, PreviousValue = previous, NewValue = newer, OriginIp = "127.0.0.1", ActionAt = date };
    private static async Task AssertSeedVisible(IServiceProvider services) { await using var scope = services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); Assert.Equal(8, await db.AuditLogs.CountAsync()); }
    private static async Task<Snapshot> SnapshotAsync(IServiceProvider services) { await using var scope = services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); return new(await db.AuditLogs.CountAsync(), await db.TaxClassifications.CountAsync(), await db.ClassificationHistories.CountAsync(), await db.UploadFiles.CountAsync(), await db.BulkUploadDetails.CountAsync(), await db.BulkUploadErrors.CountAsync(), await db.TaxReports.CountAsync()); }
    private sealed record Snapshot(int AuditLogs, int TaxClassifications, int ClassificationHistories, int UploadFiles, int BulkUploadDetails, int BulkUploadErrors, int TaxReports);
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

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
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
using NuamExchange.Application.BackupMetadata;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.BackupMetadata;
using NuamExchange.Infrastructure.Persistence;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Api.Tests;

public sealed class BackupMetadataQueryTests
{
    private const string SentinelBackupPath = "SENTINEL_BACKUP_PATH_DO_NOT_EXPOSE_035";
    private const string SentinelObservation = "SENTINEL_OBSERVATION_DO_NOT_EXPOSE_035";
    private static readonly string[] SentinelSensitiveValues =
    [
        SentinelBackupPath,
        SentinelObservation,
        "SENTINEL_BACKUP_PATH_TWO_DO_NOT_EXPOSE_035",
        "SENTINEL_BACKUP_PATH_THREE_DO_NOT_EXPOSE_035",
        "SENTINEL_OBSERVATION_THREE_DO_NOT_EXPOSE_035",
        "SENTINEL_BACKUP_PATH_FOUR_DO_NOT_EXPOSE_035"
    ];

    [Fact]
    public async Task Administrator_CanListBackupMetadata()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        Assert.Equal(HttpStatusCode.OK, (await factory.CreateClient().GetAsync("/api/backup-metadata")).StatusCode);
    }

    [Theory]
    [InlineData(SecuritySeedService.SupervisorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    [InlineData("Otro Rol")]
    public async Task NonAdministratorAuthenticatedUsers_ReturnForbidden(string role)
    {
        using var factory = CreateFactory(role);
        Assert.Equal(HttpStatusCode.Forbidden, (await factory.CreateClient().GetAsync("/api/backup-metadata")).StatusCode);
    }

    [Fact]
    public async Task WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, authenticated: false);
        Assert.Equal(HttpStatusCode.Unauthorized, (await factory.CreateClient().GetAsync("/api/backup-metadata")).StatusCode);
    }

    [Fact]
    public async Task List_PaginatesSortsFiltersAndExcludesSensitiveFields()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/backup-metadata?page=1&pageSize=2");
        var body = await response.Content.ReadAsStringAsync();
        var page = await response.Content.ReadFromJsonAsync<PagedResult<BackupMetadataListItemDto>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(page?.Items);
        Assert.Equal(4, page!.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Equal(2, page.Items.Count);
        Assert.Equal(new[] { 4, 3 }, page.Items.Select(x => x.Id).ToArray());
        AssertSensitiveFieldsExcluded(body, SentinelBackupPath, SentinelObservation);

        var byType = await client.GetFromJsonAsync<PagedResult<BackupMetadataListItemDto>>("/api/backup-metadata?backupType=BASE_DATOS");
        Assert.Equal(new[] { 4, 1 }, byType!.Items.Select(x => x.Id).ToArray());
        var byStatus = await client.GetFromJsonAsync<PagedResult<BackupMetadataListItemDto>>("/api/backup-metadata?status=FALLIDO");
        Assert.Single(byStatus!.Items);
        Assert.Equal(3, byStatus.Items.Single().Id);
        var byDate = await client.GetFromJsonAsync<PagedResult<BackupMetadataListItemDto>>("/api/backup-metadata?dateFrom=2026-06-02T00:00:00Z&dateTo=2026-06-03T23:59:59Z&sortBy=id&sortDirection=asc");
        Assert.Equal(new[] { 2, 3 }, byDate!.Items.Select(x => x.Id).ToArray());
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("sortBy=backupPath")]
    [InlineData("sortDirection=sideways")]
    [InlineData("dateFrom=2026-06-05T00:00:00Z&dateTo=2026-06-01T00:00:00Z")]
    [InlineData("backupType=INVENTADO")]
    [InlineData("status=INVENTADO")]
    public async Task InvalidParameters_ReturnBadRequest(string query)
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        Assert.Equal(HttpStatusCode.BadRequest, (await factory.CreateClient().GetAsync($"/api/backup-metadata?{query}")).StatusCode);
    }

    [Fact]
    public async Task Detail_ExistingReturnsSafeDto_MissingReturns404_InvalidReturns400()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/backup-metadata/1");
        var body = await response.Content.ReadAsStringAsync();
        var detail = await response.Content.ReadFromJsonAsync<BackupMetadataDetailDto>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, detail!.Id);
        Assert.True(detail.HasObservation);
        AssertSensitiveFieldsExcluded(body, SentinelBackupPath, SentinelObservation);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/backup-metadata/9999")).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync("/api/backup-metadata/0")).StatusCode);
    }


    [Fact]
    public void AssertSensitiveFieldsExcluded_AllowsHasObservationAndRejectsExactSensitivePropertiesAndSentinels()
    {
        AssertSensitiveFieldsExcluded("{ \"hasObservation\": true, \"items\": [{ \"hasObservation\": false }] }");

        Assert.NotNull(Record.Exception(() => AssertSensitiveFieldsExcluded("{ \"observation\": \"text\" }")));
        Assert.NotNull(Record.Exception(() => AssertSensitiveFieldsExcluded("{ \"observacion\": \"text\" }")));
        Assert.NotNull(Record.Exception(() => AssertSensitiveFieldsExcluded($"{{ \"backupType\": \"{SentinelBackupPath}\" }}")));
        Assert.NotNull(Record.Exception(() => AssertSensitiveFieldsExcluded($"{{ \"status\": \"{SentinelObservation}\" }}")));
    }

    [Fact]
    public async Task Query_DoesNotModifyOperationalData()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var before = await SnapshotAsync(factory.Services);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/backup-metadata")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/backup-metadata/1")).StatusCode);
        Assert.Equal(before, await SnapshotAsync(factory.Services));
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
    {
        services.RemoveAll<NuamExchangeDbContext>(); services.RemoveAll<DbContextOptions<NuamExchangeDbContext>>();
        var databaseName = Guid.NewGuid().ToString();
        services.AddDbContext<NuamExchangeDbContext>(o => o.UseInMemoryDatabase(databaseName).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        services.RemoveAll<IBackupMetadataQueryService>(); services.AddScoped<IBackupMetadataQueryService, BackupMetadataQueryService>();
        services.AddAuthentication(options => { options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme; options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme; }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, o => { o.ClaimsIssuer = authenticated ? role : "__unauthenticated__"; });
        using var sp = services.BuildServiceProvider(); using var scope = sp.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); Seed(db); db.SaveChanges();
    }));

    private static void Seed(NuamExchangeDbContext db)
    {
        db.Roles.Add(new Role { Id = 1, Name = SecuritySeedService.AdministratorRole, CreatedAt = DateTime.UtcNow });
        db.Users.Add(new ApplicationUser { Id = 1, FullName = "User", Email = "u@example.test", PasswordHash = "hash", RoleId = 1, CreatedAt = DateTime.UtcNow });
        db.TaxClassifications.Add(new TaxClassification { Id = 10, CreatorUserId = 1, Market = "BOLSA", ClassificationType = "DIVIDENDO", Currency = "CLP", TaxPeriod = 2026, ValidFrom = new DateOnly(2026, 1, 1), Status = "VIGENTE", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.UploadTemplates.Add(new UploadTemplate { Id = 1, UploadType = "X_FACTOR", TemplateName = "Plantilla X Factor", RequiredColumns = "market,classification_type,currency,tax_period,valid_from,factor", AllowedFormat = "CSV", TemplateVersion = "1.0", CreatedAt = DateTime.UtcNow });
        db.UploadFiles.Add(new UploadFile { Id = 1, UserId = 1, UploadTemplateId = 1, UploadType = "X_FACTOR", FileName = "archivo.csv", Extension = "CSV", FilePath = "/private/archivo.csv", FileHash = "hash", UploadStatus = "PROCESADO", UploadedAt = DateTime.UtcNow });
        db.BackupRecords.AddRange(
            Backup(1, "BASE_DATOS", "EJECUTADO", new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc), SentinelBackupPath, SentinelObservation),
            Backup(2, "ARCHIVOS", "PROGRAMADO", new DateTime(2026, 6, 2, 12, 0, 0, DateTimeKind.Utc), "SENTINEL_BACKUP_PATH_TWO_DO_NOT_EXPOSE_035", null),
            Backup(3, "COMPLETO", "FALLIDO", new DateTime(2026, 6, 3, 12, 0, 0, DateTimeKind.Utc), "SENTINEL_BACKUP_PATH_THREE_DO_NOT_EXPOSE_035", "SENTINEL_OBSERVATION_THREE_DO_NOT_EXPOSE_035"),
            Backup(4, "BASE_DATOS", "RESTAURADO", new DateTime(2026, 6, 3, 12, 0, 0, DateTimeKind.Utc), "SENTINEL_BACKUP_PATH_FOUR_DO_NOT_EXPOSE_035", null));
    }

    private static BackupRecord Backup(int id, string type, string status, DateTime date, string path, string? observation) => new() { Id = id, UserId = 1, BackupType = type, BackupStatus = status, BackupAt = date, BackupPath = path, Observation = observation };
    private static void AssertSensitiveFieldsExcluded(string body, params string[] sentinelValues)
    {
        using var document = JsonDocument.Parse(body);
        AssertNoForbiddenJsonProperties(document.RootElement);

        foreach (var sentinel in SentinelSensitiveValues.Concat(sentinelValues).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal))
        {
            Assert.DoesNotContain(sentinel, body, StringComparison.Ordinal);
        }
    }

    private static void AssertNoForbiddenJsonProperties(JsonElement element)
    {
        var forbiddenProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "observation", "observacion", "backupPath", "rutaRespaldo", "ruta_respaldo", "ruta",
            "user", "userId", "usuario", "usuarioId", "usuario_id", "email", "password",
            "token", "jwt", "connectionString", "connection string", "hash", "size", "tamaño",
            "file", "filePath", "fileName", "archivo", "path", "secret"
        };

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    Assert.DoesNotContain(property.Name, forbiddenProperties);
                    AssertNoForbiddenJsonProperties(property.Value);
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray()) AssertNoForbiddenJsonProperties(item);
                break;
        }
    }
    private static async Task<Snapshot> SnapshotAsync(IServiceProvider services) { await using var scope = services.CreateAsyncScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); return new(await db.BackupRecords.CountAsync(), await db.AuditLogs.CountAsync(), await db.TaxClassifications.CountAsync(), await db.UploadFiles.CountAsync(), await db.Users.CountAsync()); }
    private sealed record Snapshot(int BackupRecords, int AuditLogs, int TaxClassifications, int UploadFiles, int Users);
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

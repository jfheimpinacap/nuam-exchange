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

namespace NuamExchange.Api.Tests;

public sealed class BulkLoadQueryTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    [InlineData(SecuritySeedService.SupervisorRole)]
    public async Task GetBulkLoads_WithReadRole_ReturnsOk(string role)
    {
        using var factory = CreateFactory(role);
        using var response = await factory.CreateClient().GetAsync("/api/bulk-loads");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBulkLoads_WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, authenticated: false);
        using var response = await factory.CreateClient().GetAsync("/api/bulk-loads");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("/api/bulk-loads?page=0")]
    [InlineData("/api/bulk-loads?pageSize=0")]
    [InlineData("/api/bulk-loads?sortBy=filePath")]
    public async Task GetBulkLoads_WithInvalidQuery_ReturnsBadRequest(string url)
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var response = await factory.CreateClient().GetAsync(url);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetBulkLoads_AppliesFiltersPaginationOrderingAndDoesNotExposeInternalFields()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var response = await factory.CreateClient().GetAsync("/api/bulk-loads?uploadType=X_MONTO&status=PROCESADO_CON_ERRORES&page=1&pageSize=1&sortBy=fileName&sortDirection=asc");
        var body = await response.Content.ReadAsStringAsync();
        var result = await response.Content.ReadFromJsonAsync<PagedResult<BulkLoadSummaryDto>>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result!.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, x => Assert.Equal("X_MONTO", x.UploadType));
        Assert.DoesNotContain("filePath", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("fileHash", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("/var/private", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetBulkLoadById_MapsExistingAndMissing()
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole);
        var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/bulk-loads/1")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/bulk-loads/999")).StatusCode);
    }

    [Fact]
    public async Task DetailsAndErrors_AreIsolatedByUploadAndPaginated()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        var client = factory.CreateClient();
        var details = await (await client.GetAsync("/api/bulk-loads/1/details?page=1&pageSize=1")).Content.ReadFromJsonAsync<PagedResult<BulkLoadDetailDto>>();
        var errors = await (await client.GetAsync("/api/bulk-loads/1/errors?page=1&pageSize=1")).Content.ReadFromJsonAsync<PagedResult<BulkLoadErrorDto>>();
        Assert.NotNull(details); Assert.Single(details!.Items); Assert.Equal(2, details.TotalCount); Assert.All(details.Items, x => Assert.Equal(1, x.UploadFileId));
        Assert.NotNull(errors); Assert.Single(errors!.Items); Assert.Equal(1, errors.TotalCount); Assert.All(errors.Items, x => Assert.Equal(1, x.UploadFileId));
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/bulk-loads/999/details")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/bulk-loads/999/errors")).StatusCode);
    }

    [Fact]
    public async Task EmptyListDetailsAndErrors_ReturnOkWithEmptyCollections()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, seed: false);
        var client = factory.CreateClient();
        var list = await (await client.GetAsync("/api/bulk-loads")).Content.ReadFromJsonAsync<PagedResult<BulkLoadSummaryDto>>();
        Assert.Empty(list!.Items);

        await SeedSingleUploadWithoutChildren(factory.Services);
        var details = await (await client.GetAsync("/api/bulk-loads/10/details")).Content.ReadFromJsonAsync<PagedResult<BulkLoadDetailDto>>();
        var errors = await (await client.GetAsync("/api/bulk-loads/10/errors")).Content.ReadFromJsonAsync<PagedResult<BulkLoadErrorDto>>();
        Assert.Empty(details!.Items);
        Assert.Empty(errors!.Items);
    }

    [Fact]
    public async Task QueryService_DoesNotModifyTrackedData()
    {
        var options = new DbContextOptionsBuilder<NuamExchangeDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)).Options;
        await using var db = new NuamExchangeDbContext(options); Seed(db); await db.SaveChangesAsync();
        var service = new BulkLoadQueryService(db);
        await service.GetAsync(new ValidatedBulkLoadQuery(1, 20, null, null, null, null, "uploadedAt", "desc"));
        await service.GetByIdAsync(1);
        await service.GetDetailsAsync(1, new ValidatedBulkLoadDetailQuery(1, 20, null, null, null, null));
        await service.GetErrorsAsync(1, new ValidatedBulkLoadErrorQuery(1, 20, null, null, null));
        Assert.False(db.ChangeTracker.Entries().Any(e => e.State is EntityState.Modified or EntityState.Added or EntityState.Deleted));
        Assert.Equal(3, await db.UploadFiles.CountAsync());
        Assert.Equal(3, await db.BulkUploadDetails.CountAsync());
        Assert.Equal(2, await db.BulkUploadErrors.CountAsync());
        Assert.Equal(1, await db.TaxClassifications.CountAsync());
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true, bool seed = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<NuamExchangeDbContext>(); services.RemoveAll<DbContextOptions<NuamExchangeDbContext>>();
            services.AddDbContext<NuamExchangeDbContext>(o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()).ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
            services.RemoveAll<IBulkLoadQueryService>(); services.AddScoped<IBulkLoadQueryService, BulkLoadQueryService>();
            services.AddAuthentication(options => { options.DefaultAuthenticateScheme = TestAuthenticationHandler.AuthenticationScheme; options.DefaultChallengeScheme = TestAuthenticationHandler.AuthenticationScheme; }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthenticationHandler.AuthenticationScheme, o => { o.ClaimsIssuer = authenticated ? role : "__unauthenticated__"; });
            if (seed) { using var sp = services.BuildServiceProvider(); using var scope = sp.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>(); Seed(db); db.SaveChanges(); }
        });
    });

    private static async Task SeedSingleUploadWithoutChildren(IServiceProvider services)
    {
        using var scope = services.CreateScope(); var db = scope.ServiceProvider.GetRequiredService<NuamExchangeDbContext>();
        var t = new UploadTemplate { Id = 10, UploadType = "X_FACTOR", TemplateName = "Factor", RequiredColumns = "a", AllowedFormat = "CSV", TemplateVersion = "1.0", CreatedAt = DateTime.UtcNow };
        db.UploadTemplates.Add(t); db.UploadFiles.Add(new UploadFile { Id = 10, UserId = 1, UploadTemplateId = 10, UploadTemplate = t, UploadType = "X_FACTOR", FileName = "empty.csv", Extension = "CSV", FilePath = "/var/private/empty.csv", FileHash = "hash", UploadStatus = "PROCESADO", UploadedAt = DateTime.UtcNow }); await db.SaveChangesAsync();
    }

    private static void Seed(NuamExchangeDbContext db)
    {
        var user = new ApplicationUser { Id = 1, FullName = "User", Email = "u@example.test", PasswordHash = "hash", RoleId = 1, CreatedAt = DateTime.UtcNow };
        var role = new Role { Id = 1, Name = "Administrador", CreatedAt = DateTime.UtcNow };
        user.Role = role; db.Roles.Add(role); db.Users.Add(user);
        var template = new UploadTemplate { Id = 1, UploadType = "X_FACTOR", TemplateName = "Factor", RequiredColumns = "market", AllowedFormat = "CSV", TemplateVersion = "1.0", CreatedAt = DateTime.UtcNow };
        var template2 = new UploadTemplate { Id = 2, UploadType = "X_MONTO", TemplateName = "Monto", RequiredColumns = "market", AllowedFormat = "CSV", TemplateVersion = "1.0", CreatedAt = DateTime.UtcNow };
        db.UploadTemplates.AddRange(template, template2);
        db.TaxClassifications.Add(new TaxClassification { Id = 1, CreatorUserId = 1, Market = "BOLSA", ClassificationType = "DIV", Currency = "CLP", TaxPeriod = 2026, ValidFrom = new DateOnly(2026, 1, 1), Status = "ACTIVA", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        db.UploadFiles.AddRange(
            new UploadFile { Id = 1, UserId = 1, UploadTemplateId = 1, UploadTemplate = template, UploadType = "X_FACTOR", FileName = "a.csv", Extension = "CSV", FilePath = "/var/private/a.csv", FileHash = "secret", FileSizeBytes = 10, UploadStatus = "PROCESADO", TotalRecords = 2, ValidRecords = 1, ErrorRecords = 1, UploadedAt = DateTime.UtcNow.AddDays(-2) },
            new UploadFile { Id = 2, UserId = 1, UploadTemplateId = 2, UploadTemplate = template2, UploadType = "X_MONTO", FileName = "b.csv", Extension = "CSV", FilePath = "/var/private/b.csv", FileHash = "secret", UploadStatus = "PROCESADO_CON_ERRORES", TotalRecords = 1, ValidRecords = 0, ErrorRecords = 1, UploadedAt = DateTime.UtcNow.AddDays(-1) },
            new UploadFile { Id = 3, UserId = 1, UploadTemplateId = 2, UploadTemplate = template2, UploadType = "X_MONTO", FileName = "c.csv", Extension = "CSV", FilePath = "/var/private/c.csv", FileHash = "secret", UploadStatus = "PROCESADO_CON_ERRORES", TotalRecords = 1, ValidRecords = 0, ErrorRecords = 1, UploadedAt = DateTime.UtcNow });
        db.BulkUploadDetails.AddRange(new BulkUploadDetail { Id = 1, UploadFileId = 1, TaxClassificationId = 1, RowNumber = 2, AffectedField = "AppliedFactor", FactorValue = 1.2m, OriginalTextValue = "1.2", RowStatus = "APLICADA", CreatedAt = DateTime.UtcNow }, new BulkUploadDetail { Id = 2, UploadFileId = 1, RowNumber = 3, AffectedField = "AppliedFactor", RowStatus = "CON_ERROR", CreatedAt = DateTime.UtcNow }, new BulkUploadDetail { Id = 3, UploadFileId = 2, RowNumber = 2, AffectedField = "ReferenceAmount", AmountValue = 10, RowStatus = "APLICADA", CreatedAt = DateTime.UtcNow });
        db.BulkUploadErrors.AddRange(new BulkUploadError { Id = 1, UploadFileId = 1, RowNumber = 3, ColumnName = "appliedFactor", ErrorDescription = "Valor inválido.", Severity = "ERROR", CreatedAt = DateTime.UtcNow }, new BulkUploadError { Id = 2, UploadFileId = 2, RowNumber = 2, ColumnName = "referenceAmount", ErrorDescription = "Valor inválido.", Severity = "ERROR", CreatedAt = DateTime.UtcNow });
    }

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

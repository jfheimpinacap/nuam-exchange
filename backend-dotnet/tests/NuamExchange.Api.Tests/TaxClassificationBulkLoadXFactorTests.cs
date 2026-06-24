using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

public sealed class TaxClassificationBulkLoadXFactorTests
{
    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    public async Task BulkLoadEndpoint_WithWriteRoleAndMultipartFile_ReturnsOk(string role)
    {
        using var factory = CreateFactory(role);
        using var client = factory.CreateClient();
        using var form = CreateCsvForm("market;instrumentCode;taxPeriod;appliedFactor\nBOLSA;NUAM;2026;1.25\n");

        using var response = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<BulkLoadXFactorResult>();
        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalRows);
        Assert.Equal(1, result.SuccessfulRows);
        Assert.Equal(0, result.FailedRows);
    }

    [Fact]
    public async Task BulkLoadEndpoint_WithSupervisor_ReturnsForbidden()
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole);
        using var client = factory.CreateClient();
        using var form = CreateCsvForm("market;instrumentCode;taxPeriod;appliedFactor\nBOLSA;NUAM;2026;1.25\n");

        using var response = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BulkLoadEndpoint_WithoutJwt_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole, authenticated: false);
        using var client = factory.CreateClient();
        using var form = CreateCsvForm("market;instrumentCode;taxPeriod;appliedFactor\nBOLSA;NUAM;2026;1.25\n");

        using var response = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task BulkLoadEndpoint_MissingFileField_ReturnsBadRequest()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();

        using var response = await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task BulkLoadEndpoint_RejectsEmptyNonCsvInvalidHeaderAndJsonAlternative()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var client = factory.CreateClient();

        using var empty = CreateCsvForm(string.Empty);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", empty)).StatusCode);

        using var nonCsv = CreateForm("file", "data", "x-factor.txt", "text/plain");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", nonCsv)).StatusCode);

        using var badHeader = CreateCsvForm("mercado;codigo;periodo;factor\nBOLSA;NUAM;2026;1.25\n");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", badHeader)).StatusCode);

        using var json = JsonContent.Create(new { file = "market;instrumentCode;taxPeriod;appliedFactor" });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/api/tax-classifications/bulk-loads/x-factor", json)).StatusCode);
    }

    [Fact]
    public async Task BulkLoadService_UpdatesOnlyFactorAndTraceabilityForValidRowsAndErrorsForInvalidRows()
    {
        var options = CreateOptions();
        await using var dbContext = new NuamExchangeDbContext(options);
        var created = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        dbContext.TaxClassifications.AddRange(
            CreateTax(1, "BOLSA", "NUAM-1", 2026, 1m, created),
            CreateTax(2, "BOLSA", "DUP", 2026, 2m, created),
            CreateTax(3, "BOLSA", "DUP", 2026, 3m, created),
            CreateTax(4, "BOLSA", "BAD-FIRST", 2026, 4m, created));
        await dbContext.SaveChangesAsync();

        var service = new TaxClassificationCommandService(dbContext);
        var csv = "market;instrumentCode;taxPeriod;appliedFactor\nBOLSA;NUAM-1;2026;1.25000000\nBOLSA;NOPE;2026;4.0\nBOLSA;DUP;2026;5.0\nBOLSA;BAD-FIRST;2026;bad\nBOLSA;NUAM-1;2026;6.0\nBOLSA;BAD-FIRST;2026;7.50000000\n";
        var result = await service.BulkLoadXFactorAsync(new BulkLoadXFactorCommand(99, "x-factor.csv", Encoding.UTF8.GetByteCount(csv), csv, "127.0.0.1"));

        Assert.Equal(6, result.TotalRows);
        Assert.Equal(2, result.SuccessfulRows);
        Assert.Equal(4, result.FailedRows);
        Assert.Contains(1, result.UpdatedTaxClassificationIds);
        Assert.Contains(4, result.UpdatedTaxClassificationIds);
        Assert.Contains(result.Errors, x => x.RowNumber == 3 && x.Code == "NOT_FOUND");
        Assert.Contains(result.Errors, x => x.RowNumber == 4 && x.Code == "AMBIGUOUS_MATCH");
        Assert.Contains(result.Errors, x => x.RowNumber == 5 && x.Code == "INVALID_APPLIED_FACTOR");
        Assert.Contains(result.Errors, x => x.RowNumber == 6 && x.Code == "DUPLICATE_ROW");

        var updated = await dbContext.TaxClassifications.SingleAsync(x => x.Id == 1);
        Assert.Equal(1.25m, updated.AppliedFactor);
        Assert.True(updated.UpdatedAt > created);
        Assert.Equal("VIGENTE", updated.Status);
        Assert.Equal(10, updated.CreatorUserId);
        Assert.Equal(created, updated.CreatedAt);
        Assert.Equal("BOLSA", updated.Market);
        Assert.Equal("NUAM-1", updated.InstrumentCode);

        var invalidThenValid = await dbContext.TaxClassifications.SingleAsync(x => x.Id == 4);
        Assert.Equal(7.5m, invalidThenValid.AppliedFactor);
        Assert.True(invalidThenValid.UpdatedAt > created);

        var ambiguousRows = await dbContext.TaxClassifications.Where(x => x.InstrumentCode == "DUP").ToListAsync();
        Assert.All(ambiguousRows, x =>
        {
            Assert.Equal(created, x.UpdatedAt);
            Assert.Contains<decimal?>(x.AppliedFactor, new decimal?[] { 2m, 3m });
        });

        Assert.Equal(4, await dbContext.TaxClassifications.CountAsync());
        Assert.Single(dbContext.UploadFiles);
        Assert.Equal(6, await dbContext.BulkUploadDetails.CountAsync());
        Assert.Equal(4, await dbContext.BulkUploadErrors.CountAsync());
        Assert.Equal(2, await dbContext.ClassificationHistories.CountAsync());
        Assert.All(await dbContext.ClassificationHistories.ToListAsync(), history =>
        {
            Assert.Equal("MODIFICACION", history.ChangeType);
            Assert.Equal("AppliedFactor", history.ModifiedField);
        });
        Assert.Equal(2, await dbContext.AuditLogs.CountAsync());
        Assert.All(await dbContext.AuditLogs.ToListAsync(), audit => Assert.Equal("TAX_CLASSIFICATION_FACTOR_BULK_UPDATED", audit.Action));
    }

    private static DbContextOptions<NuamExchangeDbContext> CreateOptions() => new DbContextOptionsBuilder<NuamExchangeDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
        .Options;

    private static TaxClassification CreateTax(int id, string market, string code, int period, decimal factor, DateTime created) => new()
    {
        Id = id,
        CreatorUserId = 10,
        Market = market,
        InstrumentCode = code,
        InstrumentName = "Instrumento",
        ClassificationType = "DIVIDENDO",
        Description = "Base",
        UpdatePercentage = 0m,
        AppliedFactor = factor,
        ReferenceAmount = 100m,
        Currency = "CLP",
        TaxPeriod = period,
        ValidFrom = new DateOnly(period, 1, 1),
        ValidTo = null,
        Status = "VIGENTE",
        CreatedAt = created,
        UpdatedAt = created
    };

    private static MultipartFormDataContent CreateCsvForm(string content) => CreateForm("file", content, "x-factor.csv", "text/csv");

    private static MultipartFormDataContent CreateForm(string fieldName, string content, string fileName, string mediaType)
    {
        var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(Encoding.UTF8.GetBytes(content)) { Headers = { ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mediaType) } }, fieldName, fileName);
        return form;
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ITaxClassificationCommandService>();
            services.AddScoped<ITaxClassificationCommandService, FakeBulkLoadCommandService>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RoleAuthenticationHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = RoleAuthenticationHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, RoleAuthenticationHandler>(RoleAuthenticationHandler.AuthenticationScheme, options => { options.ClaimsIssuer = authenticated ? role : string.Empty; });
        });
    });

    private sealed class FakeBulkLoadCommandService : ITaxClassificationCommandService
    {
        public Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<SupervisorValidationResult> SupervisorValidationAsync(ValidatedSupervisorValidationTaxClassificationCommand command, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<BulkLoadXFactorResult> BulkLoadXFactorAsync(BulkLoadXFactorCommand command, CancellationToken cancellationToken = default)
            => Task.FromResult(new BulkLoadXFactorResult(55, 1, 1, 0, new[] { 7 }, Array.Empty<BulkLoadXFactorErrorDto>()));
    }

    private sealed class RoleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "BulkLoadRoleTest";
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

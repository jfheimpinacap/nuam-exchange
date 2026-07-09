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
using NuamExchange.Application.DocumentReviews;
using NuamExchange.Infrastructure.DocumentReviews;
using NuamExchange.Infrastructure.Seeding;

namespace NuamExchange.Api.Tests;

public sealed class DocumentReviewTests
{
    [Fact]
    public void Parser_ClassifiesValidIncompleteAndUnsupported()
    {
        var parser = new PdfTaxDocumentTextParser();
        var valid = parser.Parse("ok.pdf", 10, 1, "Tipo de documento: Certificado Tributario\nMercado: BOLSA\nInstrumento: NUAM-ACC-001\nPeriodo tributario: 2026\nFactor aplicado: 1.234\nMonto de referencia: 1000000\nFecha de emisión: 04-07-2026");
        Assert.Equal("VALID", valid.Status);
        var incomplete = parser.Parse("bad.pdf", 10, 1, "Tipo de documento: Certificado Tributario\nMercado: BOLSA\nInstrumento: NUAM-ACC-001\nPeriodo tributario: 2026\nMonto de referencia: 1000000\nFecha de emisión: 04-07-2026");
        Assert.Equal("INCOMPLETE", incomplete.Status);
        Assert.Contains("Factor aplicado", incomplete.MissingFields);
        var unsupported = parser.Parse("generic.pdf", 10, 1, "Documento generico sin formato esperado");
        Assert.Equal("UNSUPPORTED", unsupported.Status);
    }

    [Theory]
    [InlineData(SecuritySeedService.AdministratorRole)]
    [InlineData(SecuritySeedService.TaxAnalystRole)]
    public async Task Endpoint_WithAllowedRole_ReturnsOk(string role)
    {
        using var factory = CreateFactory(role);
        using var client = factory.CreateClient();
        using var form = CreateForm("demo.pdf", "application/pdf");
        using var response = await client.PostAsync("/api/document-reviews/pdf", form);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PdfDocumentReviewResult>();
        Assert.Equal("VALID", result!.Status);
    }

    [Fact]
    public async Task Endpoint_RejectsMissingNonPdfUnauthorizedAndSupervisor()
    {
        using var admin = CreateFactory(SecuritySeedService.AdministratorRole).CreateClient();
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync("/api/document-reviews/pdf", new MultipartFormDataContent())).StatusCode);
        using var txt = CreateForm("demo.txt", "text/plain");
        Assert.Equal(HttpStatusCode.BadRequest, (await admin.PostAsync("/api/document-reviews/pdf", txt)).StatusCode);

        using var anonFactory = CreateFactory(SecuritySeedService.AdministratorRole, false);
        using var anon = anonFactory.CreateClient();
        using var pdf = CreateForm("demo.pdf", "application/pdf");
        Assert.Equal(HttpStatusCode.Unauthorized, (await anon.PostAsync("/api/document-reviews/pdf", pdf)).StatusCode);

        using var supFactory = CreateFactory(SecuritySeedService.SupervisorRole);
        using var sup = supFactory.CreateClient();
        using var pdf2 = CreateForm("demo.pdf", "application/pdf");
        Assert.Equal(HttpStatusCode.Forbidden, (await sup.PostAsync("/api/document-reviews/pdf", pdf2)).StatusCode);
    }

    private static MultipartFormDataContent CreateForm(string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var content = new ByteArrayContent([1, 2, 3]);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        return form;
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder.ConfigureTestServices(services =>
    {
        services.AddAuthentication(options => { options.DefaultAuthenticateScheme = TestAuthHandler.Scheme; options.DefaultChallengeScheme = TestAuthHandler.Scheme; }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
        services.AddAuthorization(options => options.AddPolicy("DocumentReviewWrite", policy => policy.RequireRole(SecuritySeedService.AdministratorRole, SecuritySeedService.TaxAnalystRole)));
        services.RemoveAll<IPdfDocumentReviewService>();
        services.AddScoped<IPdfDocumentReviewService>(_ => new FakePdfService());
        TestAuthHandler.Role = role; TestAuthHandler.Authenticated = authenticated;
    }));

    private sealed class FakePdfService : IPdfDocumentReviewService
    {
        public Task<PdfDocumentReviewResult> ReviewAsync(PdfDocumentReviewCommand command, CancellationToken cancellationToken = default) => Task.FromResult(new PdfDocumentReviewResult(null, command.FileName, command.FileSizeBytes, 1, "VALID", "PDF válido: contiene todos los campos tributarios requeridos.", new Dictionary<string, string> { ["market"] = "BOLSA" }, Array.Empty<string>(), Array.Empty<string>(), "preview"));
    }

    private sealed class TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string Scheme = "DocumentReviewTest"; public static string Role = SecuritySeedService.AdministratorRole; public static bool Authenticated = true;
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Authenticated) return Task.FromResult(AuthenticateResult.NoResult());
            var identity = new ClaimsIdentity([new Claim("sub", "1"), new Claim(ClaimTypes.Role, Role)], Scheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme)));
        }
    }
}

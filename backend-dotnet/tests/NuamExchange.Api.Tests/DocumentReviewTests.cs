using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
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
    public void Parser_ClassifiesValidTextWithLineBreaks()
    {
        var result = ParseText("Tipo de documento: Certificado Tributario\nMercado: BOLSA\nInstrumento: NUAM-ACC-001\nPeriodo tributario: 2026\nFactor aplicado: 1.23456789\nMonto de referencia: 1000000\nFecha de emisión: 04-07-2026");

        Assert.Equal("VALID", result.Status);
        Assert.Empty(result.MissingFields);
    }

    [Fact]
    public void Parser_ClassifiesValidConcatenatedText()
    {
        var result = ParseText("Tipo de documento: Certificado TributarioMercado: BOLSAInstrumento: NUAM-ACC-001Periodo tributario: 2026Factor aplicado: 1.23456789Monto de referencia: 1000000Fecha de emisión: 04-07-2026");

        Assert.Equal("VALID", result.Status);
        Assert.Empty(result.MissingFields);
        Assert.Equal("Certificado Tributario", result.DetectedFields["documentType"]);
        Assert.Equal("BOLSA", result.DetectedFields["market"]);
    }

    [Fact]
    public void Parser_ClassifiesIncompleteConcatenatedTextWithoutAppliedFactor()
    {
        var result = ParseText("Tipo de documento: Certificado TributarioMercado: BOLSAInstrumento: NUAM-ACC-001Periodo tributario: 2026Monto de referencia: 1000000Fecha de emisión: 04-07-2026");

        Assert.Equal("INCOMPLETE", result.Status);
        Assert.Equal(["Factor aplicado"], result.MissingFields);
    }

    [Fact]
    public void Parser_ClassifiesIncompleteConcatenatedTextWithoutTaxPeriod()
    {
        var result = ParseText("Tipo de documento: Certificado TributarioMercado: BOLSAInstrumento: NUAM-ACC-001Factor aplicado: 1.23456789Monto de referencia: 1000000Fecha de emisión: 04-07-2026");

        Assert.Equal("INCOMPLETE", result.Status);
        Assert.Equal(["Periodo tributario"], result.MissingFields);
    }

    [Fact]
    public void Parser_ClassifiesNonTaxTextAsUnsupported()
    {
        var result = ParseText("Documento generico sin formato esperado");

        Assert.Equal("UNSUPPORTED", result.Status);
    }

    [Fact]
    public void Parser_DetectsDamagedIssueDateAccent()
    {
        var result = ParseText("Tipo de documento: Certificado TributarioMercado: BOLSAInstrumento: NUAM-ACC-001Período tributario: 2026Factor aplicado: 1.23456789Monto de referencia: 1000000Fecha de emisi??n: 04-07-2026");

        Assert.Equal("VALID", result.Status);
        Assert.Equal("04-07-2026", result.DetectedFields["issueDate"]);
    }

    [Fact]
    public void Parser_DoesNotConsumeFollowingFieldsAsDocumentTypeValue()
    {
        var result = ParseText("Tipo de documento: Certificado TributarioMercado: BOLSAInstrumento: NUAM-ACC-001Periodo tributario: 2026Factor aplicado: 1.23456789Monto de referencia: 1000000Fecha de emision: 04-07-2026");

        Assert.Equal("Certificado Tributario", result.DetectedFields["documentType"]);
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
        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);
        Assert.Equal("VALID", GetStatus(json.RootElement));
    }

    [Fact]
    public async Task Endpoint_MissingFile_ReturnsBadRequest()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var client = factory.CreateClient();
        using var form = new MultipartFormDataContent();

        using var response = await client.PostAsync("/api/document-reviews/pdf", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_NonPdf_ReturnsBadRequest()
    {
        using var factory = CreateFactory(SecuritySeedService.AdministratorRole);
        using var client = factory.CreateClient();
        using var form = CreateForm("demo.txt", "text/plain");

        using var response = await client.PostAsync("/api/document-reviews/pdf", form);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = CreateFactory(role: string.Empty, authenticated: false);
        using var client = factory.CreateClient();
        using var form = CreateForm("demo.pdf", "application/pdf");

        using var response = await client.PostAsync("/api/document-reviews/pdf", form);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_WithSupervisor_ReturnsForbidden()
    {
        using var factory = CreateFactory(SecuritySeedService.SupervisorRole);
        using var client = factory.CreateClient();
        using var form = CreateForm("demo.pdf", "application/pdf");

        using var response = await client.PostAsync("/api/document-reviews/pdf", form);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }


    private static PdfDocumentReviewResult ParseText(string text)
    {
        var parser = new PdfTaxDocumentTextParser();
        return parser.Parse("demo.pdf", 10, 1, text);
    }

    private static MultipartFormDataContent CreateForm(string fileName, string contentType)
    {
        var form = new MultipartFormDataContent();
        var content = new ByteArrayContent([1, 2, 3]);
        content.Headers.ContentType = new(contentType);
        form.Add(content, "file", fileName);
        return form;
    }

    private static WebApplicationFactory<Program> CreateFactory(string role, bool authenticated = true) => new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddNuamExchangeInMemoryDatabase();
            services.RemoveAll<IPdfDocumentReviewService>();
            services.AddScoped<IPdfDocumentReviewService>(_ => new FakePdfService());
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = RoleAuthenticationHandler.AuthenticationScheme;
                options.DefaultChallengeScheme = RoleAuthenticationHandler.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, RoleAuthenticationHandler>(RoleAuthenticationHandler.AuthenticationScheme, options => { options.ClaimsIssuer = authenticated ? role : string.Empty; });
        });
    });

    private static string? GetStatus(JsonElement root)
    {
        if (root.TryGetProperty("status", out var camelCaseStatus)) return camelCaseStatus.GetString();
        return root.TryGetProperty("Status", out var pascalCaseStatus) ? pascalCaseStatus.GetString() : null;
    }

    private sealed class FakePdfService : IPdfDocumentReviewService
    {
        public Task<PdfDocumentReviewResult> ReviewAsync(PdfDocumentReviewCommand command, CancellationToken cancellationToken = default) => Task.FromResult(new PdfDocumentReviewResult(null, command.FileName, command.FileSizeBytes, 1, "VALID", "PDF válido: contiene todos los campos tributarios requeridos.", new Dictionary<string, string> { ["market"] = "BOLSA" }, Array.Empty<string>(), Array.Empty<string>(), "preview"));
    }

    private sealed class RoleAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        public const string AuthenticationScheme = "DocumentReviewTest";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var role = Options.ClaimsIssuer;
            if (string.IsNullOrWhiteSpace(role)) return Task.FromResult(AuthenticateResult.NoResult());

            var claims = new[] { new Claim("sub", "1"), new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, AuthenticationScheme);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), AuthenticationScheme)));
        }
    }
}

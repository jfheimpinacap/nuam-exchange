using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace NuamExchange.Api.Tests;

public sealed class SwaggerCsvUploadDocumentationTests
{
    [Fact]
    public async Task SwaggerJson_DocumentsCsvBulkLoadsAsMultipartFileUploads()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.AddNuamExchangeInMemoryDatabase();
            });
        });
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var swagger = JsonDocument.Parse(content);
        AssertCsvUploadIsMultipartBinaryFile(swagger, "/api/tax-classifications/bulk-loads/x-factor");
        AssertCsvUploadIsMultipartBinaryFile(swagger, "/api/tax-classifications/bulk-loads/x-amount");
    }

    private static void AssertCsvUploadIsMultipartBinaryFile(JsonDocument swagger, string path)
    {
        var post = swagger.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty("post");

        var schema = post
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("multipart/form-data")
            .GetProperty("schema");

        var fileProperty = schema.GetProperty("properties").GetProperty("file");
        Assert.Equal("string", fileProperty.GetProperty("type").GetString());
        Assert.Equal("binary", fileProperty.GetProperty("format").GetString());
    }
}

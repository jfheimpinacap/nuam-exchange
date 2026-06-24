using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NuamExchange.Api.Contracts.TaxClassifications;
using Xunit;

namespace NuamExchange.Api.Tests;

public sealed class CreateTaxClassificationRequestContractTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public void Deserialize_WithCompletePublishedJson_BindsEditableFields()
    {
        const string description = "Calificación inicial creada para validar el flujo local.";
        var json = $$"""
        {
          "market": "BOLSA",
          "instrumentCode": "NUEX-PRUEBA-20260624014603090",
          "instrumentName": "Instrumento de Prueba Nuam",
          "classificationType": "DIVIDENDO",
          "description": "{{description}}",
          "updatePercentage": 0,
          "appliedFactor": 1,
          "referenceAmount": 100000,
          "currency": "CLP",
          "taxPeriod": 2026,
          "validFrom": "2026-01-01",
          "validTo": "2026-12-31"
        }
        """;

        var request = JsonSerializer.Deserialize<CreateTaxClassificationRequest>(json, JsonOptions);

        Assert.NotNull(request);
        Assert.Equal("BOLSA", request.Market);
        Assert.Equal("NUEX-PRUEBA-20260624014603090", request.InstrumentCode);
        Assert.Equal("Instrumento de Prueba Nuam", request.InstrumentName);
        Assert.Equal("DIVIDENDO", request.ClassificationType);
        Assert.Equal(description, request.Description);
        Assert.Equal(0m, request.UpdatePercentage);
        Assert.Equal(1m, request.AppliedFactor);
        Assert.Equal(100000m, request.ReferenceAmount);
        Assert.Equal("CLP", request.Currency);
        Assert.Equal(2026, request.TaxPeriod);
        Assert.Equal(new DateOnly(2026, 1, 1), request.ValidFrom);
        Assert.Equal(new DateOnly(2026, 12, 31), request.ValidTo);
    }

    [Fact]
    public void Deserialize_WithNullDescription_BindsNullDescription()
    {
        const string json = """
        {
          "market": "BOLSA",
          "instrumentCode": "NUEX-PRUEBA-NULL",
          "instrumentName": "Instrumento de Prueba Nuam",
          "classificationType": "DIVIDENDO",
          "description": null,
          "updatePercentage": 0,
          "appliedFactor": 1,
          "referenceAmount": 100000,
          "currency": "CLP",
          "taxPeriod": 2026,
          "validFrom": "2026-01-01",
          "validTo": "2026-12-31"
        }
        """;

        var request = JsonSerializer.Deserialize<CreateTaxClassificationRequest>(json, JsonOptions);

        Assert.NotNull(request);
        Assert.Null(request.Description);
    }

    [Fact]
    public void Contract_DescriptionIsOptionalNullableStringWithJsonNameAndMaxLength()
    {
        var property = typeof(CreateTaxClassificationRequest).GetProperty(nameof(CreateTaxClassificationRequest.Description));

        Assert.NotNull(property);
        Assert.Equal(typeof(string), property.PropertyType);
        Assert.Equal("description", property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        Assert.Null(property.GetCustomAttribute<RequiredAttribute>());
        Assert.Equal(500, property.GetCustomAttribute<MaxLengthAttribute>()?.Length);
    }

    [Fact]
    public void Contract_ExposesOnlyEditableCreationFieldsWithExplicitJsonNames()
    {
        var expected = new Dictionary<string, string>
        {
            [nameof(CreateTaxClassificationRequest.Market)] = "market",
            [nameof(CreateTaxClassificationRequest.InstrumentCode)] = "instrumentCode",
            [nameof(CreateTaxClassificationRequest.InstrumentName)] = "instrumentName",
            [nameof(CreateTaxClassificationRequest.ClassificationType)] = "classificationType",
            [nameof(CreateTaxClassificationRequest.Description)] = "description",
            [nameof(CreateTaxClassificationRequest.UpdatePercentage)] = "updatePercentage",
            [nameof(CreateTaxClassificationRequest.AppliedFactor)] = "appliedFactor",
            [nameof(CreateTaxClassificationRequest.ReferenceAmount)] = "referenceAmount",
            [nameof(CreateTaxClassificationRequest.Currency)] = "currency",
            [nameof(CreateTaxClassificationRequest.TaxPeriod)] = "taxPeriod",
            [nameof(CreateTaxClassificationRequest.ValidFrom)] = "validFrom",
            [nameof(CreateTaxClassificationRequest.ValidTo)] = "validTo"
        };

        var publicProperties = typeof(CreateTaxClassificationRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.Equal(expected.Keys.OrderBy(x => x), publicProperties.Select(p => p.Name).OrderBy(x => x));
        foreach (var property in publicProperties)
        {
            Assert.Equal(expected[property.Name], property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        }
    }
}

using System.Reflection;
using System.Text.Json.Serialization;
using NuamExchange.Api.Contracts.TaxClassifications;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Tests;

public sealed class UpdateTaxClassificationValidatorTests
{
    private readonly UpdateTaxClassificationValidator _validator = new();

    [Fact]
    public void Validate_RejectsValidToBeforeValidFrom()
    {
        var result = _validator.Validate(ValidCommand(validTo: new DateOnly(2025, 12, 31)));

        Assert.False(result.Succeeded);
        Assert.Equal("La fecha de término de vigencia no puede ser anterior a la fecha de inicio.", result.Message);
    }

    [Fact]
    public void Validate_NormalizesTextFields()
    {
        var result = _validator.Validate(ValidCommand(market: "  Chile  ", instrumentCode: "  ABC  ", instrumentName: "  Bono  ", classificationType: "  Renta Fija  ", description: "  Desc  ", currency: "  CLP  "));

        Assert.True(result.Succeeded);
        Assert.Equal("Chile", result.Command!.Market);
        Assert.Equal("ABC", result.Command.InstrumentCode);
        Assert.Equal("Bono", result.Command.InstrumentName);
        Assert.Equal("Renta Fija", result.Command.ClassificationType);
        Assert.Equal("Desc", result.Command.Description);
        Assert.Equal("CLP", result.Command.Currency);
    }

    [Fact]
    public void Validate_UsesRealModelLengths()
    {
        var result = _validator.Validate(ValidCommand(market: new string('M', CreateTaxClassificationValidator.MarketMaxLength + 1)));

        Assert.False(result.Succeeded);
        Assert.Equal("El mercado no puede exceder 120 caracteres.", result.Message);
    }

    [Fact]
    public void Validate_AllowsNullDescription()
    {
        var result = _validator.Validate(ValidCommand(description: null));

        Assert.True(result.Succeeded);
        Assert.Null(result.Command!.Description);
    }

    [Theory]
    [InlineData("CreatorUserId")]
    [InlineData("CreatedAt")]
    [InlineData("UpdatedAt")]
    [InlineData("Status")]
    [InlineData("Id")]
    public void UpdateRequest_DoesNotExposeSystemFields(string propertyName)
    {
        Assert.Null(typeof(UpdateTaxClassificationRequest).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase));
    }

    [Fact]
    public void UpdateRequest_UsesExplicitCamelCaseJsonPropertyNames()
    {
        var expected = new Dictionary<string, string>
        {
            [nameof(UpdateTaxClassificationRequest.Market)] = "market",
            [nameof(UpdateTaxClassificationRequest.InstrumentCode)] = "instrumentCode",
            [nameof(UpdateTaxClassificationRequest.InstrumentName)] = "instrumentName",
            [nameof(UpdateTaxClassificationRequest.ClassificationType)] = "classificationType",
            [nameof(UpdateTaxClassificationRequest.Description)] = "description",
            [nameof(UpdateTaxClassificationRequest.UpdatePercentage)] = "updatePercentage",
            [nameof(UpdateTaxClassificationRequest.AppliedFactor)] = "appliedFactor",
            [nameof(UpdateTaxClassificationRequest.ReferenceAmount)] = "referenceAmount",
            [nameof(UpdateTaxClassificationRequest.Currency)] = "currency",
            [nameof(UpdateTaxClassificationRequest.TaxPeriod)] = "taxPeriod",
            [nameof(UpdateTaxClassificationRequest.ValidFrom)] = "validFrom",
            [nameof(UpdateTaxClassificationRequest.ValidTo)] = "validTo"
        };

        var publicProperties = typeof(UpdateTaxClassificationRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        Assert.Equal(expected.Keys.OrderBy(x => x), publicProperties.Select(x => x.Name).OrderBy(x => x));
        foreach (var property in publicProperties)
        {
            Assert.Equal(expected[property.Name], property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name);
        }
    }

    private static UpdateTaxClassificationCommand ValidCommand(
        string? market = "Mercado Local",
        string? instrumentCode = null,
        string? instrumentName = null,
        string? classificationType = "Tipo",
        string? description = null,
        string? currency = null,
        DateOnly? validTo = null)
        => new(1, 7, market, instrumentCode, instrumentName, classificationType, description, null, null, null, currency, 2026, new DateOnly(2026, 1, 1), validTo, "127.0.0.1");
}

using System.Text.Json.Serialization;
using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Contracts.TaxClassifications;

public sealed class UpdateTaxClassificationRequest
{
    [JsonPropertyName("market")]
    public string? Market { get; set; }

    [JsonPropertyName("instrumentCode")]
    public string? InstrumentCode { get; set; }

    [JsonPropertyName("instrumentName")]
    public string? InstrumentName { get; set; }

    [JsonPropertyName("classificationType")]
    public string? ClassificationType { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("updatePercentage")]
    public decimal? UpdatePercentage { get; set; }

    [JsonPropertyName("appliedFactor")]
    public decimal? AppliedFactor { get; set; }

    [JsonPropertyName("referenceAmount")]
    public decimal? ReferenceAmount { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("taxPeriod")]
    public int TaxPeriod { get; set; }

    [JsonPropertyName("validFrom")]
    public DateOnly ValidFrom { get; set; }

    [JsonPropertyName("validTo")]
    public DateOnly? ValidTo { get; set; }

    public UpdateTaxClassificationCommand ToCommand(int id, int actorUserId, string? originIp) => new(
        id,
        actorUserId,
        Market,
        InstrumentCode,
        InstrumentName,
        ClassificationType,
        Description,
        UpdatePercentage,
        AppliedFactor,
        ReferenceAmount,
        Currency,
        TaxPeriod,
        ValidFrom,
        ValidTo,
        originIp);
}

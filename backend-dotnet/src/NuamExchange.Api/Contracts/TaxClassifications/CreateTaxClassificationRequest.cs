using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Contracts.TaxClassifications;

public sealed class CreateTaxClassificationRequest
{
    public string? Market { get; init; }
    public string? InstrumentCode { get; init; }
    public string? InstrumentName { get; init; }
    public string? ClassificationType { get; init; }
    public string? Description { get; init; }
    public decimal? UpdatePercentage { get; init; }
    public decimal? AppliedFactor { get; init; }
    public decimal? ReferenceAmount { get; init; }
    public string? Currency { get; init; }
    public int TaxPeriod { get; init; }
    public DateOnly ValidFrom { get; init; }
    public DateOnly? ValidTo { get; init; }

    public CreateTaxClassificationCommand ToCommand(int creatorUserId, string? originIp) => new(
        creatorUserId,
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

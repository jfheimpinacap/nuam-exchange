namespace NuamExchange.Application.TaxClassifications;

public sealed record UpdateTaxClassificationCommand(
    int Id,
    int ActorUserId,
    string? Market,
    string? InstrumentCode,
    string? InstrumentName,
    string? ClassificationType,
    string? Description,
    decimal? UpdatePercentage,
    decimal? AppliedFactor,
    decimal? ReferenceAmount,
    string? Currency,
    int TaxPeriod,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? OriginIp);

public sealed record ValidatedUpdateTaxClassificationCommand(
    int Id,
    int ActorUserId,
    string Market,
    string? InstrumentCode,
    string? InstrumentName,
    string ClassificationType,
    string? Description,
    decimal? UpdatePercentage,
    decimal? AppliedFactor,
    decimal? ReferenceAmount,
    string Currency,
    int TaxPeriod,
    DateOnly ValidFrom,
    DateOnly? ValidTo,
    string? OriginIp);

public sealed record UpdateTaxClassificationValidationResult(bool Succeeded, ValidatedUpdateTaxClassificationCommand? Command, string? Message)
{
    public static UpdateTaxClassificationValidationResult Success(ValidatedUpdateTaxClassificationCommand command) => new(true, command, null);
    public static UpdateTaxClassificationValidationResult Failure(string message) => new(false, null, message);
}

public interface IUpdateTaxClassificationValidator
{
    UpdateTaxClassificationValidationResult Validate(UpdateTaxClassificationCommand command);
}

namespace NuamExchange.Application.TaxClassifications;

public sealed record CreateTaxClassificationCommand(
    int CreatorUserId,
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

public sealed record ValidatedCreateTaxClassificationCommand(
    int CreatorUserId,
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

public sealed record CreateTaxClassificationValidationResult(bool Succeeded, ValidatedCreateTaxClassificationCommand? Command, string? Message)
{
    public static CreateTaxClassificationValidationResult Success(ValidatedCreateTaxClassificationCommand command) => new(true, command, null);
    public static CreateTaxClassificationValidationResult Failure(string message) => new(false, null, message);
}

public interface ICreateTaxClassificationValidator
{
    CreateTaxClassificationValidationResult Validate(CreateTaxClassificationCommand command);
}

public sealed record CopyTaxClassificationCommand(int SourceId, int ActorUserId, string? OriginIp);

public interface ITaxClassificationCommandService
{
    Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default);
    Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default);
    Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default);
    Task<SupervisorValidationResult> SupervisorValidationAsync(ValidatedSupervisorValidationTaxClassificationCommand command, CancellationToken cancellationToken = default);
    Task<BulkLoadXFactorResult> BulkLoadXFactorAsync(BulkLoadXFactorCommand command, CancellationToken cancellationToken = default);
}

namespace NuamExchange.Application.TaxClassifications;

public sealed record TaxClassificationListItemDto(
    int Id,
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
    string Status);

public sealed record TaxClassificationDetailDto(
    int Id,
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
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record TaxClassificationFilterOptionsDto(
    IReadOnlyCollection<string> Markets,
    IReadOnlyCollection<int> Exercises,
    IReadOnlyCollection<string> Statuses);

public sealed record PagedResult<T>(IReadOnlyCollection<T> Items, int Page, int PageSize, int TotalCount);


public sealed record TaxClassificationHistoryDto(
    int Id,
    int TaxClassificationId,
    int UserId,
    string ChangeType,
    string? ModifiedField,
    string? PreviousValue,
    string? NewValue,
    string? Observation,
    DateTime ChangedAt);

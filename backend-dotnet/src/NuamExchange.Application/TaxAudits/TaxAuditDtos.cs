using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Application.TaxAudits;

public sealed record TaxAuditQuery(int? Page = TaxAuditQueryDefaults.DefaultPage, int? PageSize = TaxAuditQueryDefaults.DefaultPageSize, int? TaxClassificationId = null, string? Action = null, DateTime? DateFrom = null, DateTime? DateTo = null, string? SortBy = TaxAuditQueryDefaults.DefaultSortBy, string SortDirection = TaxAuditQueryDefaults.DefaultSortDirection);
public static class TaxAuditQueryDefaults { public const int DefaultPage = 1; public const int DefaultPageSize = 20; public const int MaxPageSize = 100; public const string DefaultSortBy = "occurredAt"; public const string DefaultSortDirection = "desc"; }
public sealed record ValidatedTaxAuditQuery(int Page, int PageSize, int? TaxClassificationId, string? Action, DateTime? DateFrom, DateTime? DateTo, string SortBy, string SortDirection);
public sealed record TaxAuditValidationResult(bool Succeeded, ValidatedTaxAuditQuery? Query, string? Message) { public static TaxAuditValidationResult Success(ValidatedTaxAuditQuery query) => new(true, query, null); public static TaxAuditValidationResult Failure(string message) => new(false, null, message); }
public interface ITaxAuditQueryValidator { TaxAuditValidationResult Validate(TaxAuditQuery query); }
public interface ITaxAuditQueryService { Task<PagedResult<TaxAuditListItemDto>> GetAsync(ValidatedTaxAuditQuery query, CancellationToken cancellationToken = default); Task<TaxAuditDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default); }
public sealed record TaxAuditListItemDto(int Id, string Action, int TaxClassificationId, int? ActorUserId, DateTime OccurredAt);
public sealed record TaxAuditDetailDto(int Id, string Action, int TaxClassificationId, int? ActorUserId, DateTime OccurredAt, string? Detail, string? PreviousValue, string? NewValue);

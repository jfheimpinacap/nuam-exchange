using NuamExchange.Application.TaxAudits;

namespace NuamExchange.Api.Contracts.TaxAudits;

public sealed class TaxAuditRequest
{
    public int? Page { get; init; } = TaxAuditQueryDefaults.DefaultPage;
    public int? PageSize { get; init; } = TaxAuditQueryDefaults.DefaultPageSize;
    public int? TaxClassificationId { get; init; }
    public string? Action { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SortBy { get; init; } = TaxAuditQueryDefaults.DefaultSortBy;
    public string SortDirection { get; init; } = TaxAuditQueryDefaults.DefaultSortDirection;
    public TaxAuditQuery ToQuery() => new(Page, PageSize, TaxClassificationId, Action, DateFrom, DateTo, SortBy, SortDirection);
}

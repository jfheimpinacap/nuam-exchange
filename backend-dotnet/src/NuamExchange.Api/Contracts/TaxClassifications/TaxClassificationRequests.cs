using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Api.Contracts.TaxClassifications;

public sealed class TaxClassificationListRequest
{
    public string? Search { get; init; }
    public string? Market { get; init; }
    public int? Exercise { get; init; }
    public string? Status { get; init; }
    public int Page { get; init; } = TaxClassificationQueryDefaults.DefaultPage;
    public int PageSize { get; init; } = TaxClassificationQueryDefaults.DefaultPageSize;
    public string? SortBy { get; init; } = TaxClassificationQueryDefaults.DefaultSortBy;
    public string SortDirection { get; init; } = TaxClassificationQueryDefaults.DefaultSortDirection;

    public TaxClassificationQuery ToQuery() => new(Search, Market, Exercise, Status, Page, PageSize, SortBy, SortDirection);
}

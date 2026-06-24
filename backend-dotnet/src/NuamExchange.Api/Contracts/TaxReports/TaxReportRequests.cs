using NuamExchange.Application.TaxReports;

namespace NuamExchange.Api.Contracts.TaxReports;

public sealed class TaxClassificationReportRequest
{
    public string? Market { get; init; }
    public string? InstrumentCode { get; init; }
    public int? TaxPeriod { get; init; }
    public string? Status { get; init; }
    public string? ClassificationType { get; init; }
    public string? Currency { get; init; }
    public int Page { get; init; } = TaxClassificationReportDefaults.DefaultPage;
    public int PageSize { get; init; } = TaxClassificationReportDefaults.DefaultPageSize;
    public string? SortBy { get; init; } = TaxClassificationReportDefaults.DefaultSortBy;
    public string SortDirection { get; init; } = TaxClassificationReportDefaults.DefaultSortDirection;
    public TaxClassificationReportQuery ToQuery() => new(Market, InstrumentCode, TaxPeriod, Status, ClassificationType, Currency, Page, PageSize, SortBy, SortDirection);
}

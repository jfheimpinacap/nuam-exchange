namespace NuamExchange.Application.TaxClassifications;

public sealed record TaxClassificationQuery(
    string? Search,
    string? Market,
    int? Exercise,
    string? Status,
    int Page = TaxClassificationQueryDefaults.DefaultPage,
    int PageSize = TaxClassificationQueryDefaults.DefaultPageSize,
    string? SortBy = TaxClassificationQueryDefaults.DefaultSortBy,
    string SortDirection = TaxClassificationQueryDefaults.DefaultSortDirection);

public static class TaxClassificationQueryDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const string DefaultSortBy = "validFrom";
    public const string DefaultSortDirection = "desc";
}

public sealed record ValidatedTaxClassificationQuery(
    string? Search,
    string? Market,
    int? Exercise,
    string? Status,
    int Page,
    int PageSize,
    string SortBy,
    string SortDirection);

public sealed record TaxClassificationQueryValidationResult(bool Succeeded, ValidatedTaxClassificationQuery? Query, string? Message)
{
    public static TaxClassificationQueryValidationResult Success(ValidatedTaxClassificationQuery query) => new(true, query, null);
    public static TaxClassificationQueryValidationResult Failure(string message) => new(false, null, message);
}

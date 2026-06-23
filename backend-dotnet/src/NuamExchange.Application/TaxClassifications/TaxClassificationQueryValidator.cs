namespace NuamExchange.Application.TaxClassifications;

public interface ITaxClassificationQueryValidator
{
    TaxClassificationQueryValidationResult Validate(TaxClassificationQuery query);
}

public sealed class TaxClassificationQueryValidator : ITaxClassificationQueryValidator
{
    public static readonly IReadOnlySet<string> AllowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "id", "market", "instrumentCode", "instrumentName", "classificationType", "currency", "taxPeriod", "validFrom", "validTo", "status", "createdAt", "updatedAt"
    };

    public TaxClassificationQueryValidationResult Validate(TaxClassificationQuery query)
    {
        if (query.Page < 1) return TaxClassificationQueryValidationResult.Failure("page debe ser mayor o igual a 1.");
        if (query.PageSize < 1 || query.PageSize > TaxClassificationQueryDefaults.MaxPageSize) return TaxClassificationQueryValidationResult.Failure("pageSize debe estar entre 1 y 100.");

        var sortDirection = Normalize(query.SortDirection) ?? TaxClassificationQueryDefaults.DefaultSortDirection;
        if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase))
        {
            return TaxClassificationQueryValidationResult.Failure("sortDirection solo acepta asc o desc.");
        }

        var sortBy = Normalize(query.SortBy) ?? TaxClassificationQueryDefaults.DefaultSortBy;
        if (!AllowedSortFields.Contains(sortBy)) return TaxClassificationQueryValidationResult.Failure("sortBy no corresponde a un campo permitido.");

        return TaxClassificationQueryValidationResult.Success(new ValidatedTaxClassificationQuery(
            Normalize(query.Search),
            Normalize(query.Market),
            query.Exercise,
            Normalize(query.Status),
            query.Page,
            query.PageSize,
            ToCanonicalSortBy(sortBy),
            sortDirection.ToLowerInvariant()));
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static string ToCanonicalSortBy(string sortBy) => AllowedSortFields.First(x => string.Equals(x, sortBy, StringComparison.OrdinalIgnoreCase));
}

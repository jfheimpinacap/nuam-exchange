namespace NuamExchange.Application.TaxReports;

public sealed class TaxReportQueryValidator : ITaxReportQueryValidator
{
    public static readonly IReadOnlySet<string> AllowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    { "id", "market", "instrumentCode", "instrumentName", "classificationType", "taxPeriod", "status", "appliedFactor", "referenceAmount", "currency", "validFrom", "validTo", "updatedAt" };

    public TaxClassificationReportValidationResult Validate(TaxClassificationReportQuery query)
    {
        if (query.Page < 1) return TaxClassificationReportValidationResult.Failure("page debe ser mayor o igual a 1.");
        if (query.PageSize < 1) return TaxClassificationReportValidationResult.Failure("pageSize debe ser mayor o igual a 1.");
        if (query.PageSize > TaxClassificationReportDefaults.MaxPageSize) return TaxClassificationReportValidationResult.Failure("pageSize no puede superar 100.");
        var sortDirection = Normalize(query.SortDirection) ?? TaxClassificationReportDefaults.DefaultSortDirection;
        if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)) return TaxClassificationReportValidationResult.Failure("sortDirection solo acepta asc o desc.");
        var sortBy = Normalize(query.SortBy) ?? TaxClassificationReportDefaults.DefaultSortBy;
        if (!AllowedSortFields.Contains(sortBy)) return TaxClassificationReportValidationResult.Failure("sortBy no corresponde a un campo permitido.");
        return TaxClassificationReportValidationResult.Success(new ValidatedTaxClassificationReportQuery(Normalize(query.Market), Normalize(query.InstrumentCode), query.TaxPeriod, Normalize(query.Status), Normalize(query.ClassificationType), Normalize(query.Currency), query.Page, query.PageSize, AllowedSortFields.First(x => string.Equals(x, sortBy, StringComparison.OrdinalIgnoreCase)), sortDirection.ToLowerInvariant()));
    }

    private static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }
}

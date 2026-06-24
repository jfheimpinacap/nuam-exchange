namespace NuamExchange.Application.TaxAudits;

public sealed class TaxAuditQueryValidator : ITaxAuditQueryValidator
{
    public TaxAuditValidationResult Validate(TaxAuditQuery query)
    {
        var page = query.Page ?? TaxAuditQueryDefaults.DefaultPage;
        var pageSize = query.PageSize ?? TaxAuditQueryDefaults.DefaultPageSize;
        if (page < 1) return TaxAuditValidationResult.Failure("page debe ser mayor o igual a 1.");
        if (pageSize < 1) return TaxAuditValidationResult.Failure("pageSize debe ser mayor o igual a 1.");
        if (pageSize > TaxAuditQueryDefaults.MaxPageSize) return TaxAuditValidationResult.Failure("pageSize no puede superar 100.");
        if (query.TaxClassificationId is <= 0) return TaxAuditValidationResult.Failure("taxClassificationId debe ser mayor o igual a 1.");
        if (query.DateFrom is not null && query.DateTo is not null && query.DateFrom > query.DateTo) return TaxAuditValidationResult.Failure("dateFrom no puede ser posterior a dateTo.");
        var action = Normalize(query.Action);
        if (action is not null && !TaxAuditRules.AllowedActions.Contains(action)) return TaxAuditValidationResult.Failure("action no corresponde a una acción tributaria permitida.");
        var sortBy = Normalize(query.SortBy) ?? TaxAuditQueryDefaults.DefaultSortBy;
        if (!TaxAuditRules.AllowedSortFields.Contains(sortBy)) return TaxAuditValidationResult.Failure("sortBy no corresponde a un campo permitido.");
        sortBy = TaxAuditRules.AllowedSortFields.First(x => string.Equals(x, sortBy, StringComparison.OrdinalIgnoreCase));
        var sortDirection = Normalize(query.SortDirection) ?? TaxAuditQueryDefaults.DefaultSortDirection;
        if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)) return TaxAuditValidationResult.Failure("sortDirection solo acepta asc o desc.");
        return TaxAuditValidationResult.Success(new(page, pageSize, query.TaxClassificationId, action, query.DateFrom, query.DateTo, sortBy, sortDirection.ToLowerInvariant()));
    }
    private static string? Normalize(string? value) { var trimmed = value?.Trim(); return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed; }
}

namespace NuamExchange.Application.BackupMetadata;

public sealed class BackupMetadataQueryValidator : IBackupMetadataQueryValidator
{
    public BackupMetadataValidationResult Validate(BackupMetadataQuery query)
    {
        var page = query.Page ?? BackupMetadataQueryDefaults.DefaultPage;
        var pageSize = query.PageSize ?? BackupMetadataQueryDefaults.DefaultPageSize;
        if (page < 1) return BackupMetadataValidationResult.Failure("page debe ser mayor o igual a 1.");
        if (pageSize < 1) return BackupMetadataValidationResult.Failure("pageSize debe ser mayor o igual a 1.");
        if (pageSize > BackupMetadataQueryDefaults.MaxPageSize) return BackupMetadataValidationResult.Failure("pageSize no puede superar 100.");
        if (query.DateFrom is not null && query.DateTo is not null && query.DateFrom > query.DateTo) return BackupMetadataValidationResult.Failure("dateFrom no puede ser posterior a dateTo.");
        var backupType = Normalize(query.BackupType);
        if (backupType is not null && !BackupMetadataRules.AllowedBackupTypes.Contains(backupType)) return BackupMetadataValidationResult.Failure("backupType no corresponde a un valor permitido.");
        var status = Normalize(query.Status);
        if (status is not null && !BackupMetadataRules.AllowedStatuses.Contains(status)) return BackupMetadataValidationResult.Failure("status no corresponde a un valor permitido.");
        var sortBy = Normalize(query.SortBy) ?? BackupMetadataQueryDefaults.DefaultSortBy;
        if (!BackupMetadataRules.AllowedSortFields.Contains(sortBy)) return BackupMetadataValidationResult.Failure("sortBy no corresponde a un campo permitido.");
        sortBy = BackupMetadataRules.AllowedSortFields.First(x => string.Equals(x, sortBy, StringComparison.OrdinalIgnoreCase));
        var sortDirection = Normalize(query.SortDirection) ?? BackupMetadataQueryDefaults.DefaultSortDirection;
        if (!string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase) && !string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase)) return BackupMetadataValidationResult.Failure("sortDirection solo acepta asc o desc.");
        return BackupMetadataValidationResult.Success(new(backupType, status, query.DateFrom, query.DateTo, sortBy, sortDirection.ToLowerInvariant(), page, pageSize));
    }
    private static string? Normalize(string? value) { var trimmed = value?.Trim(); return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed; }
}

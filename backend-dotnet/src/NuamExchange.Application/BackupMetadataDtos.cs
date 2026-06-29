using NuamExchange.Application.TaxClassifications;

namespace NuamExchange.Application.BackupMetadata;

public sealed record BackupMetadataQuery(string? BackupType = null, string? Status = null, DateTime? DateFrom = null, DateTime? DateTo = null, string? SortBy = BackupMetadataQueryDefaults.DefaultSortBy, string SortDirection = BackupMetadataQueryDefaults.DefaultSortDirection, int? Page = BackupMetadataQueryDefaults.DefaultPage, int? PageSize = BackupMetadataQueryDefaults.DefaultPageSize);
public static class BackupMetadataQueryDefaults { public const int DefaultPage = 1; public const int DefaultPageSize = 20; public const int MaxPageSize = 100; public const string DefaultSortBy = "occurredAt"; public const string DefaultSortDirection = "desc"; }
public static class BackupMetadataRules
{
    public static readonly IReadOnlySet<string> AllowedBackupTypes = new HashSet<string>(StringComparer.Ordinal) { "BASE_DATOS", "ARCHIVOS", "COMPLETO" };
    public static readonly IReadOnlySet<string> AllowedStatuses = new HashSet<string>(StringComparer.Ordinal) { "PROGRAMADO", "EJECUTADO", "FALLIDO", "RESTAURADO" };
    public static readonly IReadOnlySet<string> AllowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "id", "backupType", "status", "occurredAt" };
}
public sealed record ValidatedBackupMetadataQuery(string? BackupType, string? Status, DateTime? DateFrom, DateTime? DateTo, string SortBy, string SortDirection, int Page, int PageSize);
public sealed record BackupMetadataValidationResult(bool Succeeded, ValidatedBackupMetadataQuery? Query, string? Message) { public static BackupMetadataValidationResult Success(ValidatedBackupMetadataQuery query) => new(true, query, null); public static BackupMetadataValidationResult Failure(string message) => new(false, null, message); }
public interface IBackupMetadataQueryValidator { BackupMetadataValidationResult Validate(BackupMetadataQuery query); }
public interface IBackupMetadataQueryService { Task<PagedResult<BackupMetadataListItemDto>> GetAsync(ValidatedBackupMetadataQuery query, CancellationToken cancellationToken = default); Task<BackupMetadataDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default); }
public sealed record BackupMetadataListItemDto(int Id, string BackupType, string Status, DateTime OccurredAt, bool HasObservation);
public sealed record BackupMetadataDetailDto(int Id, string BackupType, string Status, DateTime OccurredAt, bool HasObservation);

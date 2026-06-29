using NuamExchange.Application.BackupMetadata;

namespace NuamExchange.Api.Contracts.BackupMetadata;

public sealed class BackupMetadataRequest
{
    public string? BackupType { get; init; }
    public string? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SortBy { get; init; } = BackupMetadataQueryDefaults.DefaultSortBy;
    public string SortDirection { get; init; } = BackupMetadataQueryDefaults.DefaultSortDirection;
    public int? Page { get; init; } = BackupMetadataQueryDefaults.DefaultPage;
    public int? PageSize { get; init; } = BackupMetadataQueryDefaults.DefaultPageSize;
    public BackupMetadataQuery ToQuery() => new(BackupType, Status, DateFrom, DateTo, SortBy, SortDirection, Page, PageSize);
}

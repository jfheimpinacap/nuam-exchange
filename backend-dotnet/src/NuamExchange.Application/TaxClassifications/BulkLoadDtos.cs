namespace NuamExchange.Application.TaxClassifications;

public sealed record BulkLoadTemplateDto(int Id, string UploadType, string TemplateName, string TemplateVersion, string AllowedFormat, bool IsActive);
public sealed record BulkLoadSummaryDto(int Id, string UploadType, string FileName, string Extension, long? FileSizeBytes, string Status, int TotalRecords, int ValidRecords, int ErrorRecords, string? Observation, DateTime UploadedAt, BulkLoadTemplateDto? Template, int DetailCount, int ErrorCount);
public sealed record BulkLoadDetailDto(int Id, int UploadFileId, int? TaxClassificationId, int RowNumber, string? AffectedField, decimal? FactorValue, decimal? AmountValue, string? OriginalTextValue, string RowStatus, string? Observation, DateTime CreatedAt);
public sealed record BulkLoadErrorDto(int Id, int UploadFileId, int? RowNumber, string? ColumnName, string ErrorDescription, string Severity, DateTime CreatedAt);

public interface IBulkLoadQueryService
{
    Task<PagedResult<BulkLoadSummaryDto>> GetAsync(ValidatedBulkLoadQuery query, CancellationToken cancellationToken = default);
    Task<BulkLoadSummaryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResult<BulkLoadDetailDto>?> GetDetailsAsync(int id, ValidatedBulkLoadDetailQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<BulkLoadErrorDto>?> GetErrorsAsync(int id, ValidatedBulkLoadErrorQuery query, CancellationToken cancellationToken = default);
}

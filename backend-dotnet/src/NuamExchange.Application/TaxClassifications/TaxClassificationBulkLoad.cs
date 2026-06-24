namespace NuamExchange.Application.TaxClassifications;

public sealed record BulkLoadXFactorCommand(
    int ActorUserId,
    string FileName,
    long FileSizeBytes,
    string CsvContent,
    string? OriginIp);

public sealed record BulkLoadXFactorResult(
    int UploadId,
    int TotalRows,
    int SuccessfulRows,
    int FailedRows,
    IReadOnlyCollection<int> UpdatedTaxClassificationIds,
    IReadOnlyCollection<BulkLoadXFactorErrorDto> Errors);

public sealed record BulkLoadXFactorErrorDto(int RowNumber, string Code, string Message);

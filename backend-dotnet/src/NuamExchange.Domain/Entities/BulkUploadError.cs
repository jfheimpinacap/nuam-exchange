namespace NuamExchange.Domain.Entities;

public sealed class BulkUploadError
{
    public int Id { get; set; }
    public int UploadFileId { get; set; }
    public int? RowNumber { get; set; }
    public string? ColumnName { get; set; }
    public string ErrorDescription { get; set; } = string.Empty;
    public string Severity { get; set; } = "ERROR";
    public DateTime CreatedAt { get; set; }
    public UploadFile UploadFile { get; set; } = null!;
}

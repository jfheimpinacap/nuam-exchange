namespace NuamExchange.Domain.Entities;

public sealed class UploadFile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int UploadTemplateId { get; set; }
    public string UploadType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileHash { get; set; }
    public long? FileSizeBytes { get; set; }
    public string UploadStatus { get; set; } = "RECIBIDO";
    public int TotalRecords { get; set; }
    public int ValidRecords { get; set; }
    public int ErrorRecords { get; set; }
    public string? Observation { get; set; }
    public DateTime UploadedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public UploadTemplate UploadTemplate { get; set; } = null!;
    public ICollection<BulkUploadDetail> Details { get; } = new List<BulkUploadDetail>();
    public ICollection<BulkUploadError> Errors { get; } = new List<BulkUploadError>();
    public ICollection<TaxValidation> TaxValidations { get; } = new List<TaxValidation>();
}

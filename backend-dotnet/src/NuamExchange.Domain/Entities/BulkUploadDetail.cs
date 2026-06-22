namespace NuamExchange.Domain.Entities;

public sealed class BulkUploadDetail
{
    public int Id { get; set; }
    public int UploadFileId { get; set; }
    public int? TaxClassificationId { get; set; }
    public int RowNumber { get; set; }
    public string? AffectedField { get; set; }
    public decimal? FactorValue { get; set; }
    public decimal? AmountValue { get; set; }
    public string? OriginalTextValue { get; set; }
    public string RowStatus { get; set; } = "PENDIENTE";
    public string? Observation { get; set; }
    public DateTime CreatedAt { get; set; }
    public UploadFile UploadFile { get; set; } = null!;
    public TaxClassification? TaxClassification { get; set; }
}

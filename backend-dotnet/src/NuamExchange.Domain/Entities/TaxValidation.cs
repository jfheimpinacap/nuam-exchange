namespace NuamExchange.Domain.Entities;

public sealed class TaxValidation
{
    public int Id { get; set; }
    public int? TaxClassificationId { get; set; }
    public int? UploadFileId { get; set; }
    public int UserId { get; set; }
    public string Result { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public DateTime ValidatedAt { get; set; }
    public TaxClassification? TaxClassification { get; set; }
    public UploadFile? UploadFile { get; set; }
    public ApplicationUser User { get; set; } = null!;
}

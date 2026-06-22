namespace NuamExchange.Domain.Entities;

public sealed class TaxClassification
{
    public int Id { get; set; }
    public int CreatorUserId { get; set; }
    public string Market { get; set; } = string.Empty;
    public string? InstrumentCode { get; set; }
    public string? InstrumentName { get; set; }
    public string ClassificationType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal? UpdatePercentage { get; set; }
    public decimal? AppliedFactor { get; set; }
    public decimal? ReferenceAmount { get; set; }
    public string Currency { get; set; } = "CLP";
    public int TaxPeriod { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
    public string Status { get; set; } = "VIGENTE";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ApplicationUser CreatorUser { get; set; } = null!;
    public ICollection<ClassificationHistory> History { get; } = new List<ClassificationHistory>();
    public ICollection<BulkUploadDetail> BulkUploadDetails { get; } = new List<BulkUploadDetail>();
    public ICollection<TaxValidation> TaxValidations { get; } = new List<TaxValidation>();
}

namespace NuamExchange.Domain.Entities;

public sealed class ClassificationHistory
{
    public int Id { get; set; }
    public int TaxClassificationId { get; set; }
    public int UserId { get; set; }
    public string ChangeType { get; set; } = string.Empty;
    public string? ModifiedField { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string? Observation { get; set; }
    public DateTime ChangedAt { get; set; }
    public TaxClassification TaxClassification { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}

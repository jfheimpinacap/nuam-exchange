namespace NuamExchange.Domain.Entities;

public sealed class TaxReport
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string? AppliedFilters { get; set; }
    public string Format { get; set; } = string.Empty;
    public string? ReportPath { get; set; }
    public DateTime GeneratedAt { get; set; }
    public ApplicationUser User { get; set; } = null!;
}

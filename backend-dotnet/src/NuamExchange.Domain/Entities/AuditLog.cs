namespace NuamExchange.Domain.Entities;

public sealed class AuditLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string AffectedEntity { get; set; } = string.Empty;
    public int? AffectedRecordId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public string? OriginIp { get; set; }
    public DateTime ActionAt { get; set; }
    public ApplicationUser? User { get; set; }
}

namespace NuamExchange.Domain.Entities;

public sealed class BackupRecord
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string BackupType { get; set; } = string.Empty;
    public string BackupPath { get; set; } = string.Empty;
    public string BackupStatus { get; set; } = "PROGRAMADO";
    public DateTime BackupAt { get; set; }
    public string? Observation { get; set; }
    public ApplicationUser? User { get; set; }
}

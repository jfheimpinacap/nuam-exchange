namespace NuamExchange.Domain.Entities;

public sealed class ApplicationUser
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? LastAccessAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<TaxClassification> CreatedTaxClassifications { get; } = new List<TaxClassification>();
    public ICollection<ClassificationHistory> ClassificationHistories { get; } = new List<ClassificationHistory>();
    public ICollection<UploadFile> UploadFiles { get; } = new List<UploadFile>();
    public ICollection<TaxValidation> TaxValidations { get; } = new List<TaxValidation>();
    public ICollection<TaxReport> TaxReports { get; } = new List<TaxReport>();
    public ICollection<AuditLog> AuditLogs { get; } = new List<AuditLog>();
    public ICollection<BackupRecord> BackupRecords { get; } = new List<BackupRecord>();
}

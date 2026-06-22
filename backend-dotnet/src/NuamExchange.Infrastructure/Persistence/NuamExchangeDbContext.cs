using Microsoft.EntityFrameworkCore;
using NuamExchange.Domain.Entities;

namespace NuamExchange.Infrastructure.Persistence;

public sealed class NuamExchangeDbContext(DbContextOptions<NuamExchangeDbContext> options) : DbContext(options)
{
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<BackupRecord> BackupRecords => Set<BackupRecord>();
    public DbSet<BulkUploadDetail> BulkUploadDetails => Set<BulkUploadDetail>();
    public DbSet<BulkUploadError> BulkUploadErrors => Set<BulkUploadError>();
    public DbSet<ClassificationHistory> ClassificationHistories => Set<ClassificationHistory>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<TaxClassification> TaxClassifications => Set<TaxClassification>();
    public DbSet<TaxReport> TaxReports => Set<TaxReport>();
    public DbSet<TaxValidation> TaxValidations => Set<TaxValidation>();
    public DbSet<UploadFile> UploadFiles => Set<UploadFile>();
    public DbSet<UploadTemplate> UploadTemplates => Set<UploadTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NuamExchangeDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

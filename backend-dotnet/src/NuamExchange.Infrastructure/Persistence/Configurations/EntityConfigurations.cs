using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NuamExchange.Domain.Entities;

namespace NuamExchange.Infrastructure.Persistence.Configurations;

internal static class SqlDefaults
{
    public const string CurrentDateTime = "SYSDATETIME()";
    public const DeleteBehavior NoAction = DeleteBehavior.NoAction;
}

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("Rol");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("rol_id");
        b.Property(x => x.Name).HasColumnName("nombre").HasMaxLength(80).IsRequired();
        b.Property(x => x.Description).HasColumnName("descripcion").HasMaxLength(250);
        b.Property(x => x.IsActive).HasColumnName("activo").HasDefaultValue(true).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("Permiso");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("permiso_id");
        b.Property(x => x.Code).HasColumnName("codigo").HasMaxLength(120).IsRequired();
        b.Property(x => x.Description).HasColumnName("descripcion").HasMaxLength(300);
        b.HasIndex(x => x.Code).IsUnique();
    }
}

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.ToTable("RolPermiso");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("rol_permiso_id");
        b.Property(x => x.RoleId).HasColumnName("rol_id").IsRequired();
        b.Property(x => x.PermissionId).HasColumnName("permiso_id").IsRequired();
        b.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
        b.HasOne(x => x.Role).WithMany(x => x.RolePermissions).HasForeignKey(x => x.RoleId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.Permission).WithMany(x => x.RolePermissions).HasForeignKey(x => x.PermissionId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.ToTable("Usuario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("usuario_id");
        b.Property(x => x.RoleId).HasColumnName("rol_id").IsRequired();
        b.Property(x => x.FullName).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(180).IsRequired();
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
        b.Property(x => x.JobTitle).HasColumnName("cargo").HasMaxLength(120);
        b.Property(x => x.IsActive).HasColumnName("activo").HasDefaultValue(true).IsRequired();
        b.Property(x => x.LastAccessAt).HasColumnName("ultimo_acceso_en").HasColumnType("datetime2(0)");
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("actualizado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.RoleId);
        b.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class TaxClassificationConfiguration : IEntityTypeConfiguration<TaxClassification>
{
    public void Configure(EntityTypeBuilder<TaxClassification> b)
    {
        b.ToTable("CalificacionTributaria", t =>
        {
            t.HasCheckConstraint("CK_CalificacionTributaria_PeriodoTributario", "[periodo_tributario] BETWEEN 2000 AND 2100");
            t.HasCheckConstraint("CK_CalificacionTributaria_FactorAplicado", "[factor_aplicado] IS NULL OR [factor_aplicado] >= 0");
            t.HasCheckConstraint("CK_CalificacionTributaria_PorcentajeActualizacion", "[porcentaje_actualizacion] IS NULL OR [porcentaje_actualizacion] >= 0");
            t.HasCheckConstraint("CK_CalificacionTributaria_MontoReferencia", "[monto_referencia] IS NULL OR [monto_referencia] >= 0");
            t.HasCheckConstraint("CK_CalificacionTributaria_Vigencia", "[vigente_hasta] IS NULL OR [vigente_desde] <= [vigente_hasta]");
            t.HasCheckConstraint("CK_CalificacionTributaria_Estado", "[estado_calificacion] IN ('BORRADOR','VIGENTE','OBSERVADA','REEMPLAZADA','ANULADA')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("calificacion_id");
        b.Property(x => x.CreatorUserId).HasColumnName("usuario_creador_id").IsRequired();
        b.Property(x => x.Market).HasColumnName("mercado").HasMaxLength(120).IsRequired();
        b.Property(x => x.InstrumentCode).HasColumnName("codigo_instrumento").HasMaxLength(80);
        b.Property(x => x.InstrumentName).HasColumnName("nombre_instrumento").HasMaxLength(180);
        b.Property(x => x.ClassificationType).HasColumnName("tipo_calificacion").HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(x => x.UpdatePercentage).HasColumnName("porcentaje_actualizacion").HasColumnType("decimal(10,4)");
        b.Property(x => x.AppliedFactor).HasColumnName("factor_aplicado").HasColumnType("decimal(18,8)");
        b.Property(x => x.ReferenceAmount).HasColumnName("monto_referencia").HasColumnType("decimal(18,4)");
        b.Property(x => x.Currency).HasColumnName("moneda").HasMaxLength(10).HasDefaultValue("CLP").IsRequired();
        b.Property(x => x.TaxPeriod).HasColumnName("periodo_tributario").IsRequired();
        b.Property(x => x.ValidFrom).HasColumnName("vigente_desde").HasColumnType("date").IsRequired();
        b.Property(x => x.ValidTo).HasColumnName("vigente_hasta").HasColumnType("date");
        b.Property(x => x.Status).HasColumnName("estado_calificacion").HasMaxLength(40).HasDefaultValue("VIGENTE").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.Property(x => x.UpdatedAt).HasColumnName("actualizado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.TaxPeriod, x.Status });
        b.HasIndex(x => new { x.Market, x.ClassificationType });
        b.HasIndex(x => new { x.ValidFrom, x.ValidTo });
        b.HasOne(x => x.CreatorUser).WithMany(x => x.CreatedTaxClassifications).HasForeignKey(x => x.CreatorUserId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class ClassificationHistoryConfiguration : IEntityTypeConfiguration<ClassificationHistory>
{
    public void Configure(EntityTypeBuilder<ClassificationHistory> b)
    {
        b.ToTable("HistorialCalificacion", t => t.HasCheckConstraint("CK_HistorialCalificacion_TipoCambio", "[tipo_cambio] IN ('CREACION','MODIFICACION','ANULACION','REEMPLAZO','OBSERVACION','APROBACION')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("historial_id");
        b.Property(x => x.TaxClassificationId).HasColumnName("calificacion_id").IsRequired();
        b.Property(x => x.UserId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.ChangeType).HasColumnName("tipo_cambio").HasMaxLength(80).IsRequired();
        b.Property(x => x.ModifiedField).HasColumnName("campo_modificado").HasMaxLength(120);
        b.Property(x => x.PreviousValue).HasColumnName("valor_anterior").HasColumnType("nvarchar(max)");
        b.Property(x => x.NewValue).HasColumnName("valor_nuevo").HasColumnType("nvarchar(max)");
        b.Property(x => x.Observation).HasColumnName("observacion").HasMaxLength(700);
        b.Property(x => x.ChangedAt).HasColumnName("fecha_cambio").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.TaxClassificationId, x.ChangedAt }).IsDescending(false, true);
        b.HasOne(x => x.TaxClassification).WithMany(x => x.History).HasForeignKey(x => x.TaxClassificationId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.User).WithMany(x => x.ClassificationHistories).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class UploadTemplateConfiguration : IEntityTypeConfiguration<UploadTemplate>
{
    public void Configure(EntityTypeBuilder<UploadTemplate> b)
    {
        b.ToTable("PlantillaCarga", t =>
        {
            t.HasCheckConstraint("CK_PlantillaCarga_TipoCarga", "[tipo_carga] IN ('X_FACTOR','X_MONTO')");
            t.HasCheckConstraint("CK_PlantillaCarga_FormatoPermitido", "[formato_permitido] IN ('CSV','XLSX','CSV/XLSX')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("plantilla_id");
        b.Property(x => x.UploadType).HasColumnName("tipo_carga").HasMaxLength(40).IsRequired();
        b.Property(x => x.TemplateName).HasColumnName("nombre_plantilla").HasMaxLength(150).IsRequired();
        b.Property(x => x.Description).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(x => x.RequiredColumns).HasColumnName("columnas_requeridas").HasColumnType("nvarchar(max)").IsRequired();
        b.Property(x => x.AllowedFormat).HasColumnName("formato_permitido").HasMaxLength(80).IsRequired();
        b.Property(x => x.TemplateVersion).HasColumnName("version_plantilla").HasMaxLength(30).HasDefaultValue("1.0").IsRequired();
        b.Property(x => x.IsActive).HasColumnName("activa").HasDefaultValue(true).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.UploadType, x.TemplateVersion }).IsUnique();
        b.HasIndex(x => new { x.UploadType, x.IsActive });
    }
}

public sealed class UploadFileConfiguration : IEntityTypeConfiguration<UploadFile>
{
    public void Configure(EntityTypeBuilder<UploadFile> b)
    {
        b.ToTable("ArchivoCarga", t =>
        {
            t.HasCheckConstraint("CK_ArchivoCarga_TipoCarga", "[tipo_carga] IN ('X_FACTOR','X_MONTO')");
            t.HasCheckConstraint("CK_ArchivoCarga_Extension", "[extension] IN ('CSV','XLSX')");
            t.HasCheckConstraint("CK_ArchivoCarga_EstadoCarga", "[estado_carga] IN ('RECIBIDO','EN_VALIDACION','PROCESADO','PROCESADO_CON_ERRORES','OBSERVADO','RECHAZADO')");
            t.HasCheckConstraint("CK_ArchivoCarga_Contadores", "[total_registros] >= 0 AND [registros_validos] >= 0 AND [registros_con_error] >= 0");
            t.HasCheckConstraint("CK_ArchivoCarga_Tamanio", "[tamanio_bytes] IS NULL OR [tamanio_bytes] > 0");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("archivo_carga_id");
        b.Property(x => x.UserId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.UploadTemplateId).HasColumnName("plantilla_id").IsRequired();
        b.Property(x => x.UploadType).HasColumnName("tipo_carga").HasMaxLength(40).IsRequired();
        b.Property(x => x.FileName).HasColumnName("nombre_archivo").HasMaxLength(255).IsRequired();
        b.Property(x => x.Extension).HasColumnName("extension").HasMaxLength(20).IsRequired();
        b.Property(x => x.FilePath).HasColumnName("ruta_archivo").HasMaxLength(600).IsRequired();
        b.Property(x => x.FileHash).HasColumnName("hash_archivo").HasMaxLength(128);
        b.Property(x => x.FileSizeBytes).HasColumnName("tamanio_bytes");
        b.Property(x => x.UploadStatus).HasColumnName("estado_carga").HasMaxLength(40).HasDefaultValue("RECIBIDO").IsRequired();
        b.Property(x => x.TotalRecords).HasColumnName("total_registros").HasDefaultValue(0).IsRequired();
        b.Property(x => x.ValidRecords).HasColumnName("registros_validos").HasDefaultValue(0).IsRequired();
        b.Property(x => x.ErrorRecords).HasColumnName("registros_con_error").HasDefaultValue(0).IsRequired();
        b.Property(x => x.Observation).HasColumnName("observacion").HasMaxLength(700);
        b.Property(x => x.UploadedAt).HasColumnName("fecha_carga").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.UserId, x.UploadedAt }).IsDescending(false, true);
        b.HasIndex(x => new { x.UploadType, x.UploadStatus });
        b.HasOne(x => x.User).WithMany(x => x.UploadFiles).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.UploadTemplate).WithMany(x => x.UploadFiles).HasForeignKey(x => x.UploadTemplateId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class BulkUploadDetailConfiguration : IEntityTypeConfiguration<BulkUploadDetail>
{
    public void Configure(EntityTypeBuilder<BulkUploadDetail> b)
    {
        b.ToTable("DetalleCargaMasiva", t =>
        {
            t.HasCheckConstraint("CK_DetalleCargaMasiva_NumeroFila", "[numero_fila] > 0");
            t.HasCheckConstraint("CK_DetalleCargaMasiva_ValorFactor", "[valor_factor] IS NULL OR [valor_factor] >= 0");
            t.HasCheckConstraint("CK_DetalleCargaMasiva_ValorMonto", "[valor_monto] IS NULL OR [valor_monto] >= 0");
            t.HasCheckConstraint("CK_DetalleCargaMasiva_EstadoFila", "[estado_fila] IN ('PENDIENTE','VALIDA','CON_ERROR','APLICADA','IGNORADA')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("detalle_carga_id");
        b.Property(x => x.UploadFileId).HasColumnName("archivo_carga_id").IsRequired();
        b.Property(x => x.TaxClassificationId).HasColumnName("calificacion_id");
        b.Property(x => x.RowNumber).HasColumnName("numero_fila").IsRequired();
        b.Property(x => x.AffectedField).HasColumnName("campo_afectado").HasMaxLength(120);
        b.Property(x => x.FactorValue).HasColumnName("valor_factor").HasColumnType("decimal(18,8)");
        b.Property(x => x.AmountValue).HasColumnName("valor_monto").HasColumnType("decimal(18,4)");
        b.Property(x => x.OriginalTextValue).HasColumnName("valor_texto_original").HasMaxLength(500);
        b.Property(x => x.RowStatus).HasColumnName("estado_fila").HasMaxLength(40).HasDefaultValue("PENDIENTE").IsRequired();
        b.Property(x => x.Observation).HasColumnName("observacion").HasMaxLength(700);
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.UploadFileId, x.RowNumber });
        b.HasIndex(x => x.TaxClassificationId);
        b.HasOne(x => x.UploadFile).WithMany(x => x.Details).HasForeignKey(x => x.UploadFileId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.TaxClassification).WithMany(x => x.BulkUploadDetails).HasForeignKey(x => x.TaxClassificationId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class BulkUploadErrorConfiguration : IEntityTypeConfiguration<BulkUploadError>
{
    public void Configure(EntityTypeBuilder<BulkUploadError> b)
    {
        b.ToTable("ErrorCargaMasiva", t =>
        {
            t.HasCheckConstraint("CK_ErrorCargaMasiva_NumeroFila", "[numero_fila] IS NULL OR [numero_fila] > 0");
            t.HasCheckConstraint("CK_ErrorCargaMasiva_Severidad", "[severidad] IN ('ADVERTENCIA','ERROR','CRITICO')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("error_carga_id");
        b.Property(x => x.UploadFileId).HasColumnName("archivo_carga_id").IsRequired();
        b.Property(x => x.RowNumber).HasColumnName("numero_fila");
        b.Property(x => x.ColumnName).HasColumnName("columna").HasMaxLength(120);
        b.Property(x => x.ErrorDescription).HasColumnName("descripcion_error").HasMaxLength(800).IsRequired();
        b.Property(x => x.Severity).HasColumnName("severidad").HasMaxLength(40).HasDefaultValue("ERROR").IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("creado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.UploadFileId, x.RowNumber });
        b.HasOne(x => x.UploadFile).WithMany(x => x.Errors).HasForeignKey(x => x.UploadFileId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class TaxValidationConfiguration : IEntityTypeConfiguration<TaxValidation>
{
    public void Configure(EntityTypeBuilder<TaxValidation> b)
    {
        b.ToTable("ValidacionTributaria", t =>
        {
            t.HasCheckConstraint("CK_ValidacionTributaria_Referencia", "[calificacion_id] IS NOT NULL OR [archivo_carga_id] IS NOT NULL");
            t.HasCheckConstraint("CK_ValidacionTributaria_Resultado", "[resultado] IN ('VALIDADO','OBSERVADO','RECHAZADO','APROBADO')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("validacion_id");
        b.Property(x => x.TaxClassificationId).HasColumnName("calificacion_id");
        b.Property(x => x.UploadFileId).HasColumnName("archivo_carga_id");
        b.Property(x => x.UserId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.Result).HasColumnName("resultado").HasMaxLength(40).IsRequired();
        b.Property(x => x.Observation).HasColumnName("observacion").HasMaxLength(800);
        b.Property(x => x.ValidatedAt).HasColumnName("fecha_validacion").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.TaxClassificationId, x.ValidatedAt }).IsDescending(false, true);
        b.HasIndex(x => new { x.UploadFileId, x.ValidatedAt }).IsDescending(false, true);
        b.HasOne(x => x.TaxClassification).WithMany(x => x.TaxValidations).HasForeignKey(x => x.TaxClassificationId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.UploadFile).WithMany(x => x.TaxValidations).HasForeignKey(x => x.UploadFileId).OnDelete(SqlDefaults.NoAction);
        b.HasOne(x => x.User).WithMany(x => x.TaxValidations).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class TaxReportConfiguration : IEntityTypeConfiguration<TaxReport>
{
    public void Configure(EntityTypeBuilder<TaxReport> b)
    {
        b.ToTable("ReporteTributario", t => t.HasCheckConstraint("CK_ReporteTributario_Formato", "[formato] IN ('PDF','XLSX','CSV')"));
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("reporte_id");
        b.Property(x => x.UserId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.ReportType).HasColumnName("tipo_reporte").HasMaxLength(100).IsRequired();
        b.Property(x => x.AppliedFilters).HasColumnName("filtros_aplicados").HasColumnType("nvarchar(max)");
        b.Property(x => x.Format).HasColumnName("formato").HasMaxLength(20).IsRequired();
        b.Property(x => x.ReportPath).HasColumnName("ruta_reporte").HasMaxLength(600);
        b.Property(x => x.GeneratedAt).HasColumnName("generado_en").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.UserId, x.GeneratedAt }).IsDescending(false, true);
        b.HasOne(x => x.User).WithMany(x => x.TaxReports).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("Auditoria");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("auditoria_id");
        b.Property(x => x.UserId).HasColumnName("usuario_id");
        b.Property(x => x.AffectedEntity).HasColumnName("entidad_afectada").HasMaxLength(120).IsRequired();
        b.Property(x => x.AffectedRecordId).HasColumnName("registro_afectado_id");
        b.Property(x => x.Action).HasColumnName("accion").HasMaxLength(80).IsRequired();
        b.Property(x => x.Detail).HasColumnName("detalle").HasMaxLength(900);
        b.Property(x => x.PreviousValue).HasColumnName("valor_anterior").HasColumnType("nvarchar(max)");
        b.Property(x => x.NewValue).HasColumnName("valor_nuevo").HasColumnType("nvarchar(max)");
        b.Property(x => x.OriginIp).HasColumnName("ip_origen").HasMaxLength(60);
        b.Property(x => x.ActionAt).HasColumnName("fecha_accion").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.HasIndex(x => new { x.AffectedEntity, x.ActionAt }).IsDescending(false, true);
        b.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
    }
}

public sealed class BackupRecordConfiguration : IEntityTypeConfiguration<BackupRecord>
{
    public void Configure(EntityTypeBuilder<BackupRecord> b)
    {
        b.ToTable("Respaldo", t =>
        {
            t.HasCheckConstraint("CK_Respaldo_TipoRespaldo", "[tipo_respaldo] IN ('BASE_DATOS','ARCHIVOS','COMPLETO')");
            t.HasCheckConstraint("CK_Respaldo_EstadoRespaldo", "[estado_respaldo] IN ('PROGRAMADO','EJECUTADO','FALLIDO','RESTAURADO')");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("respaldo_id");
        b.Property(x => x.UserId).HasColumnName("usuario_id");
        b.Property(x => x.BackupType).HasColumnName("tipo_respaldo").HasMaxLength(50).IsRequired();
        b.Property(x => x.BackupPath).HasColumnName("ruta_respaldo").HasMaxLength(600).IsRequired();
        b.Property(x => x.BackupStatus).HasColumnName("estado_respaldo").HasMaxLength(40).HasDefaultValue("PROGRAMADO").IsRequired();
        b.Property(x => x.BackupAt).HasColumnName("fecha_respaldo").HasColumnType("datetime2(0)").HasDefaultValueSql(SqlDefaults.CurrentDateTime).IsRequired();
        b.Property(x => x.Observation).HasColumnName("observacion").HasMaxLength(700);
        b.HasOne(x => x.User).WithMany(x => x.BackupRecords).HasForeignKey(x => x.UserId).OnDelete(SqlDefaults.NoAction);
    }
}

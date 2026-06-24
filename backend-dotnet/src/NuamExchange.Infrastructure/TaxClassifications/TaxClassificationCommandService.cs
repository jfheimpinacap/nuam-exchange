using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxClassifications;

public sealed class TaxClassificationCommandService(NuamExchangeDbContext dbContext) : ITaxClassificationCommandService
{
    private const string InitialStatus = "VIGENTE";
    private const string InitialHistoryChangeType = "CREACION";
    private const string UpdateHistoryChangeType = "MODIFICACION";
    private const string CopyAuditAction = "TAX_CLASSIFICATION_COPIED";
    private const string ValidationAuditAction = "TAX_CLASSIFICATION_VALIDATED";
    private const string BulkFactorAuditAction = "TAX_CLASSIFICATION_FACTOR_BULK_UPDATED";

    public async Task<TaxClassificationDetailDto> CreateAsync(ValidatedCreateTaxClassificationCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var entity = new TaxClassification
        {
            CreatorUserId = command.CreatorUserId,
            Market = command.Market,
            InstrumentCode = command.InstrumentCode,
            InstrumentName = command.InstrumentName,
            ClassificationType = command.ClassificationType,
            Description = command.Description,
            UpdatePercentage = command.UpdatePercentage,
            AppliedFactor = command.AppliedFactor,
            ReferenceAmount = command.ReferenceAmount,
            Currency = command.Currency,
            TaxPeriod = command.TaxPeriod,
            ValidFrom = command.ValidFrom,
            ValidTo = command.ValidTo,
            Status = InitialStatus,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.TaxClassifications.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ClassificationHistories.Add(new ClassificationHistory
        {
            TaxClassificationId = entity.Id,
            UserId = command.CreatorUserId,
            ChangeType = InitialHistoryChangeType,
            Observation = "Creación inicial de calificación tributaria.",
            ChangedAt = now
        });

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = command.CreatorUserId,
            AffectedEntity = "CalificacionTributaria",
            AffectedRecordId = entity.Id,
            Action = "TAX_CLASSIFICATION_CREATED",
            Detail = $"Calificación tributaria {entity.Id} creada.",
            OriginIp = command.OriginIp,
            ActionAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TaxClassificationDetailDto(entity.Id, entity.CreatorUserId, entity.Market, entity.InstrumentCode, entity.InstrumentName, entity.ClassificationType, entity.Description, entity.UpdatePercentage, entity.AppliedFactor, entity.ReferenceAmount, entity.Currency, entity.TaxPeriod, entity.ValidFrom, entity.ValidTo, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task<TaxClassificationDetailDto?> UpdateAsync(ValidatedUpdateTaxClassificationCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var entity = await dbContext.TaxClassifications.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity is null) return null;

        var now = DateTime.UtcNow;
        entity.Market = command.Market;
        entity.InstrumentCode = command.InstrumentCode;
        entity.InstrumentName = command.InstrumentName;
        entity.ClassificationType = command.ClassificationType;
        entity.Description = command.Description;
        entity.UpdatePercentage = command.UpdatePercentage;
        entity.AppliedFactor = command.AppliedFactor;
        entity.ReferenceAmount = command.ReferenceAmount;
        entity.Currency = command.Currency;
        entity.TaxPeriod = command.TaxPeriod;
        entity.ValidFrom = command.ValidFrom;
        entity.ValidTo = command.ValidTo;
        entity.UpdatedAt = now;

        dbContext.ClassificationHistories.Add(new ClassificationHistory
        {
            TaxClassificationId = entity.Id,
            UserId = command.ActorUserId,
            ChangeType = UpdateHistoryChangeType,
            ModifiedField = "CamposEditables",
            Observation = "Actualización de campos editables de calificación tributaria.",
            ChangedAt = now
        });

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = command.ActorUserId,
            AffectedEntity = "CalificacionTributaria",
            AffectedRecordId = entity.Id,
            Action = "TAX_CLASSIFICATION_UPDATED",
            Detail = $"Calificación tributaria {entity.Id} actualizada.",
            OriginIp = command.OriginIp,
            ActionAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TaxClassificationDetailDto(entity.Id, entity.CreatorUserId, entity.Market, entity.InstrumentCode, entity.InstrumentName, entity.ClassificationType, entity.Description, entity.UpdatePercentage, entity.AppliedFactor, entity.ReferenceAmount, entity.Currency, entity.TaxPeriod, entity.ValidFrom, entity.ValidTo, entity.Status, entity.CreatedAt, entity.UpdatedAt);
    }

    public async Task<TaxClassificationDetailDto?> CopyAsync(CopyTaxClassificationCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var source = await dbContext.TaxClassifications
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == command.SourceId, cancellationToken);
        if (source is null) return null;

        var now = DateTime.UtcNow;
        var copy = new TaxClassification
        {
            CreatorUserId = command.ActorUserId,
            Market = source.Market,
            InstrumentCode = source.InstrumentCode,
            InstrumentName = source.InstrumentName,
            ClassificationType = source.ClassificationType,
            Description = source.Description,
            UpdatePercentage = source.UpdatePercentage,
            AppliedFactor = source.AppliedFactor,
            ReferenceAmount = source.ReferenceAmount,
            Currency = source.Currency,
            TaxPeriod = source.TaxPeriod,
            ValidFrom = source.ValidFrom,
            ValidTo = source.ValidTo,
            Status = InitialStatus,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.TaxClassifications.Add(copy);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.ClassificationHistories.Add(new ClassificationHistory
        {
            TaxClassificationId = copy.Id,
            UserId = command.ActorUserId,
            ChangeType = InitialHistoryChangeType,
            Observation = $"Calificación creada como copia de la calificación tributaria ID {source.Id}.",
            ChangedAt = now
        });

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = command.ActorUserId,
            AffectedEntity = "CalificacionTributaria",
            AffectedRecordId = copy.Id,
            Action = CopyAuditAction,
            Detail = $"Calificación tributaria {copy.Id} creada como copia de la calificación tributaria {source.Id}.",
            OriginIp = command.OriginIp,
            ActionAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TaxClassificationDetailDto(copy.Id, copy.CreatorUserId, copy.Market, copy.InstrumentCode, copy.InstrumentName, copy.ClassificationType, copy.Description, copy.UpdatePercentage, copy.AppliedFactor, copy.ReferenceAmount, copy.Currency, copy.TaxPeriod, copy.ValidFrom, copy.ValidTo, copy.Status, copy.CreatedAt, copy.UpdatedAt);
    }

    public async Task<SupervisorValidationResult> SupervisorValidationAsync(ValidatedSupervisorValidationTaxClassificationCommand command, CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var entity = await dbContext.TaxClassifications.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (entity is null) return SupervisorValidationResult.Failure(404, "La calificación tributaria no existe.");

        var transition = ResolveTransition(entity.Status, command.Decision);
        if (transition is null)
        {
            return SupervisorValidationResult.Failure(409, "La decisión no está permitida desde el estado actual de la calificación tributaria.");
        }

        var previousStatus = entity.Status;
        var now = DateTime.UtcNow;

        dbContext.TaxValidations.Add(new TaxValidation
        {
            TaxClassificationId = entity.Id,
            UserId = command.ActorUserId,
            Result = command.Decision,
            Observation = command.Observation,
            ValidatedAt = now
        });

        entity.Status = transition.Value.NewStatus;
        entity.UpdatedAt = now;

        dbContext.ClassificationHistories.Add(new ClassificationHistory
        {
            TaxClassificationId = entity.Id,
            UserId = command.ActorUserId,
            ChangeType = transition.Value.HistoryChangeType,
            ModifiedField = "Status",
            PreviousValue = previousStatus,
            NewValue = entity.Status,
            Observation = command.Observation ?? $"Validación supervisora con resultado {command.Decision}.",
            ChangedAt = now
        });

        dbContext.AuditLogs.Add(new AuditLog
        {
            UserId = command.ActorUserId,
            AffectedEntity = "CalificacionTributaria",
            AffectedRecordId = entity.Id,
            Action = ValidationAuditAction,
            Detail = $"Calificación tributaria {entity.Id} validada por supervisor.",
            PreviousValue = previousStatus,
            NewValue = entity.Status,
            OriginIp = command.OriginIp,
            ActionAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return SupervisorValidationResult.Success(new TaxClassificationDetailDto(entity.Id, entity.CreatorUserId, entity.Market, entity.InstrumentCode, entity.InstrumentName, entity.ClassificationType, entity.Description, entity.UpdatePercentage, entity.AppliedFactor, entity.ReferenceAmount, entity.Currency, entity.TaxPeriod, entity.ValidFrom, entity.ValidTo, entity.Status, entity.CreatedAt, entity.UpdatedAt));
    }


    public async Task<BulkLoadXFactorResult> BulkLoadXFactorAsync(BulkLoadXFactorCommand command, CancellationToken cancellationToken = default)
    {
        var lines = command.CsvContent.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        var rows = lines.Skip(1).Select((line, index) => new { Line = line, RowNumber = index + 2 }).Where(x => !string.IsNullOrWhiteSpace(x.Line)).ToList();
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var template = await dbContext.UploadTemplates.FirstOrDefaultAsync(x => x.UploadType == "X_FACTOR" && x.TemplateVersion == "1.0", cancellationToken);
        if (template is null)
        {
            template = new UploadTemplate
            {
                UploadType = "X_FACTOR",
                TemplateName = "Carga Masiva X Factor CSV",
                Description = "Plantilla lógica para carga masiva X Factor mediante CSV.",
                RequiredColumns = "market;instrumentCode;taxPeriod;appliedFactor",
                AllowedFormat = "CSV",
                TemplateVersion = "1.0",
                IsActive = true,
                CreatedAt = now
            };
            dbContext.UploadTemplates.Add(template);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        var upload = new UploadFile
        {
            UserId = command.ActorUserId,
            UploadTemplateId = template.Id,
            UploadType = "X_FACTOR",
            FileName = Truncate(command.FileName, 255),
            Extension = "CSV",
            FilePath = string.Empty,
            FileSizeBytes = command.FileSizeBytes,
            UploadStatus = "EN_VALIDACION",
            UploadedAt = now
        };
        dbContext.UploadFiles.Add(upload);
        await dbContext.SaveChangesAsync(cancellationToken);

        var errors = new List<BulkLoadXFactorErrorDto>();
        var updatedIds = new List<int>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var columns = row.Line.Split(';');
            var original = Truncate(row.Line, 500);
            if (columns.Length != 4)
            {
                AddFailed(upload.Id, row.RowNumber, null, null, original, "STRUCTURE", "La fila debe contener exactamente 4 columnas separadas por punto y coma.", errors, now);
                continue;
            }

            var market = columns[0].Trim();
            var instrumentCode = columns[1].Trim();
            var taxPeriodText = columns[2].Trim();
            var factorText = columns[3].Trim();
            var key = $"{market}\u001f{instrumentCode}\u001f{taxPeriodText}";

            if (string.IsNullOrWhiteSpace(market) || string.IsNullOrWhiteSpace(instrumentCode) || string.IsNullOrWhiteSpace(taxPeriodText) || string.IsNullOrWhiteSpace(factorText))
            {
                AddFailed(upload.Id, row.RowNumber, null, null, original, "REQUIRED_FIELD", "market, instrumentCode, taxPeriod y appliedFactor son obligatorios.", errors, now);
                continue;
            }
            if (!seen.Add(key))
            {
                AddFailed(upload.Id, row.RowNumber, null, null, original, "DUPLICATE_ROW", "La identidad market + instrumentCode + taxPeriod ya fue informada en el archivo.", errors, now);
                continue;
            }
            if (!int.TryParse(taxPeriodText, NumberStyles.None, CultureInfo.InvariantCulture, out var taxPeriod))
            {
                AddFailed(upload.Id, row.RowNumber, null, null, original, "INVALID_TAX_PERIOD", "taxPeriod debe ser un número entero válido.", errors, now);
                continue;
            }
            if (!TryParseFactor(factorText, out var factor) || factor.Value < 0 || factor.Value >= 10000000000m || decimal.Round(factor.Value, 8) != factor.Value)
            {
                AddFailed(upload.Id, row.RowNumber, null, null, original, "INVALID_APPLIED_FACTOR", "appliedFactor debe ser decimal válido, no negativo y compatible con decimal(18,8).", errors, now);
                continue;
            }

            var matches = await dbContext.TaxClassifications.Where(x => x.Market == market && x.InstrumentCode == instrumentCode && x.TaxPeriod == taxPeriod).Take(2).ToListAsync(cancellationToken);
            if (matches.Count == 0)
            {
                AddFailed(upload.Id, row.RowNumber, null, factor, original, "NOT_FOUND", "No existe una calificación tributaria para market + instrumentCode + taxPeriod.", errors, now);
                continue;
            }
            if (matches.Count > 1)
            {
                AddFailed(upload.Id, row.RowNumber, null, factor, original, "AMBIGUOUS_MATCH", "Existe más de una calificación tributaria para market + instrumentCode + taxPeriod.", errors, now);
                continue;
            }

            var entity = matches[0];
            var previous = entity.AppliedFactor;
            entity.AppliedFactor = factor;
            entity.UpdatedAt = now;
            dbContext.BulkUploadDetails.Add(new BulkUploadDetail { UploadFileId = upload.Id, TaxClassificationId = entity.Id, RowNumber = row.RowNumber, AffectedField = "AppliedFactor", FactorValue = factor, OriginalTextValue = original, RowStatus = "APLICADA", Observation = "Factor aplicado actualizado por Carga Masiva X Factor.", CreatedAt = now });
            dbContext.ClassificationHistories.Add(new ClassificationHistory { TaxClassificationId = entity.Id, UserId = command.ActorUserId, ChangeType = "MODIFICACION", ModifiedField = "AppliedFactor", PreviousValue = FormatDecimal(previous), NewValue = FormatDecimal(factor), Observation = "Modificación proveniente de Carga Masiva X Factor.", ChangedAt = now });
            dbContext.AuditLogs.Add(new AuditLog { UserId = command.ActorUserId, AffectedEntity = "CalificacionTributaria", AffectedRecordId = entity.Id, Action = BulkFactorAuditAction, Detail = $"Calificación tributaria {entity.Id} actualizada por Carga Masiva X Factor.", PreviousValue = FormatDecimal(previous), NewValue = FormatDecimal(factor), OriginIp = command.OriginIp, ActionAt = now });
            updatedIds.Add(entity.Id);
        }

        upload.TotalRecords = rows.Count;
        upload.ValidRecords = updatedIds.Count;
        upload.ErrorRecords = errors.Count;
        upload.UploadStatus = errors.Count == 0 ? "PROCESADO" : "PROCESADO_CON_ERRORES";
        upload.Observation = errors.Count == 0 ? "Carga Masiva X Factor procesada correctamente." : "Carga Masiva X Factor procesada con errores de fila.";

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new BulkLoadXFactorResult(upload.Id, upload.TotalRecords, upload.ValidRecords, upload.ErrorRecords, updatedIds, errors);
    }

    private static (string NewStatus, string HistoryChangeType)? ResolveTransition(string currentStatus, string decision)
        => (currentStatus, decision) switch
        {
            ("VIGENTE", "OBSERVADO") => ("OBSERVADA", "OBSERVACION"),
            ("OBSERVADA", "VALIDADO") => ("VIGENTE", "APROBACION"),
            ("OBSERVADA", "APROBADO") => ("VIGENTE", "APROBACION"),
            _ => null
        };


    private void AddFailed(int uploadId, int rowNumber, int? taxClassificationId, decimal? factor, string? original, string code, string message, List<BulkLoadXFactorErrorDto> errors, DateTime now)
    {
        errors.Add(new BulkLoadXFactorErrorDto(rowNumber, code, message));
        dbContext.BulkUploadDetails.Add(new BulkUploadDetail { UploadFileId = uploadId, TaxClassificationId = taxClassificationId, RowNumber = rowNumber, AffectedField = "AppliedFactor", FactorValue = factor, OriginalTextValue = original, RowStatus = "CON_ERROR", Observation = Truncate(message, 700), CreatedAt = now });
        dbContext.BulkUploadErrors.Add(new BulkUploadError { UploadFileId = uploadId, RowNumber = rowNumber, ColumnName = code == "INVALID_APPLIED_FACTOR" ? "appliedFactor" : null, ErrorDescription = Truncate(message, 800), Severity = "ERROR", CreatedAt = now });
    }

    private static bool TryParseFactor(string text, out decimal? value)
    {
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)) { value = invariant; return true; }
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.GetCultureInfo("es-CL"), out var cl)) { value = cl; return true; }
        value = null;
        return false;
    }

    private static string? FormatDecimal(decimal? value) => value?.ToString("0.########", CultureInfo.InvariantCulture);
    private static string Truncate(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}

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

    private static (string NewStatus, string HistoryChangeType)? ResolveTransition(string currentStatus, string decision)
        => (currentStatus, decision) switch
        {
            ("VIGENTE", "OBSERVADO") => ("OBSERVADA", "OBSERVACION"),
            ("OBSERVADA", "VALIDADO") => ("VIGENTE", "APROBACION"),
            ("OBSERVADA", "APROBADO") => ("VIGENTE", "APROBACION"),
            _ => null
        };
}

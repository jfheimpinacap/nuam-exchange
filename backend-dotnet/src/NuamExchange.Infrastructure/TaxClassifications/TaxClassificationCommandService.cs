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
}

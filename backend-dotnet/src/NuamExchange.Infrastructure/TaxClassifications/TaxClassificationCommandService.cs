using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxClassifications;

public sealed class TaxClassificationCommandService(NuamExchangeDbContext dbContext) : ITaxClassificationCommandService
{
    private const string InitialStatus = "VIGENTE";
    private const string InitialHistoryChangeType = "CREACION";

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
}

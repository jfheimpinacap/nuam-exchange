using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxAudits;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxAudits;

public sealed class TaxAuditQueryService(NuamExchangeDbContext dbContext) : ITaxAuditQueryService
{
    public async Task<PagedResult<TaxAuditListItemDto>> GetAsync(ValidatedTaxAuditQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(ApplyTaxAuditScope(dbContext.AuditLogs.AsNoTracking()), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await ApplyOrdering(source, query.SortBy, query.SortDirection)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new TaxAuditListItemDto(x.Id, x.Action, x.AffectedRecordId!.Value, x.UserId, x.ActionAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<TaxAuditListItemDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<TaxAuditDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => ApplyTaxAuditScope(dbContext.AuditLogs.AsNoTracking())
            .Where(x => x.Id == id)
            .Select(x => new TaxAuditDetailDto(x.Id, x.Action, x.AffectedRecordId!.Value, x.UserId, x.ActionAt, x.Detail, x.PreviousValue, x.NewValue))
            .SingleOrDefaultAsync(cancellationToken);

    private static IQueryable<AuditLog> ApplyTaxAuditScope(IQueryable<AuditLog> query)
        => query.Where(x => x.AffectedEntity == TaxAuditRules.TaxClassificationEntity && x.AffectedRecordId != null && TaxAuditRules.AllowedActions.Contains(x.Action));

    private static IQueryable<AuditLog> ApplyFilters(IQueryable<AuditLog> query, ValidatedTaxAuditQuery filter)
    {
        if (filter.TaxClassificationId is not null) query = query.Where(x => x.AffectedRecordId == filter.TaxClassificationId);
        if (filter.Action is not null) query = query.Where(x => x.Action == filter.Action);
        if (filter.DateFrom is not null) query = query.Where(x => x.ActionAt >= filter.DateFrom);
        if (filter.DateTo is not null) query = query.Where(x => x.ActionAt <= filter.DateTo);
        return query;
    }

    private static IOrderedQueryable<AuditLog> ApplyOrdering(IQueryable<AuditLog> query, string sortBy, string direction)
    {
        var desc = direction == "desc";
        return sortBy switch
        {
            "id" => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "action" => desc ? query.OrderByDescending(x => x.Action).ThenByDescending(x => x.ActionAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.Action).ThenBy(x => x.ActionAt).ThenBy(x => x.Id),
            "taxClassificationId" => desc ? query.OrderByDescending(x => x.AffectedRecordId).ThenByDescending(x => x.ActionAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.AffectedRecordId).ThenBy(x => x.ActionAt).ThenBy(x => x.Id),
            "actorUserId" => desc ? query.OrderByDescending(x => x.UserId).ThenByDescending(x => x.ActionAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.UserId).ThenBy(x => x.ActionAt).ThenBy(x => x.Id),
            _ => desc ? query.OrderByDescending(x => x.ActionAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.ActionAt).ThenBy(x => x.Id)
        };
    }
}

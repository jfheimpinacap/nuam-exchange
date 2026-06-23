using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxClassifications;

public sealed class TaxClassificationQueryService(NuamExchangeDbContext dbContext) : ITaxClassificationQueryService
{
    public async Task<PagedResult<TaxClassificationListItemDto>> GetAsync(ValidatedTaxClassificationQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(dbContext.TaxClassifications.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await ApplyOrdering(source, query.SortBy, query.SortDirection)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new TaxClassificationListItemDto(x.Id, x.Market, x.InstrumentCode, x.InstrumentName, x.ClassificationType, x.Description, x.UpdatePercentage, x.AppliedFactor, x.ReferenceAmount, x.Currency, x.TaxPeriod, x.ValidFrom, x.ValidTo, x.Status))
            .ToListAsync(cancellationToken);

        return new PagedResult<TaxClassificationListItemDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<TaxClassificationDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => dbContext.TaxClassifications.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new TaxClassificationDetailDto(x.Id, x.CreatorUserId, x.Market, x.InstrumentCode, x.InstrumentName, x.ClassificationType, x.Description, x.UpdatePercentage, x.AppliedFactor, x.ReferenceAmount, x.Currency, x.TaxPeriod, x.ValidFrom, x.ValidTo, x.Status, x.CreatedAt, x.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<TaxClassificationFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default)
    {
        var markets = await dbContext.TaxClassifications.AsNoTracking()
            .Select(x => x.Market.Trim())
            .Where(x => x != string.Empty)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var exercises = await dbContext.TaxClassifications.AsNoTracking()
            .Select(x => x.TaxPeriod)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var statuses = await dbContext.TaxClassifications.AsNoTracking()
            .Select(x => x.Status.Trim())
            .Where(x => x != string.Empty)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new TaxClassificationFilterOptionsDto(markets, exercises, statuses);
    }

    private static IQueryable<TaxClassification> ApplyFilters(IQueryable<TaxClassification> queryable, ValidatedTaxClassificationQuery query)
    {
        if (query.Search is not null)
        {
            queryable = queryable.Where(x =>
                (x.InstrumentCode != null && x.InstrumentCode.Contains(query.Search)) ||
                (x.InstrumentName != null && x.InstrumentName.Contains(query.Search)) ||
                (x.Description != null && x.Description.Contains(query.Search)) ||
                x.ClassificationType.Contains(query.Search));
        }

        if (query.Market is not null) queryable = queryable.Where(x => x.Market == query.Market);
        if (query.Exercise is not null) queryable = queryable.Where(x => x.TaxPeriod == query.Exercise.Value);
        if (query.Status is not null) queryable = queryable.Where(x => x.Status == query.Status);
        return queryable;
    }

    private static IOrderedQueryable<TaxClassification> ApplyOrdering(IQueryable<TaxClassification> queryable, string sortBy, string sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return sortBy switch
        {
            "id" => descending ? queryable.OrderByDescending(x => x.Id) : queryable.OrderBy(x => x.Id),
            "market" => descending ? queryable.OrderByDescending(x => x.Market).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.Market).ThenBy(x => x.Id),
            "instrumentCode" => descending ? queryable.OrderByDescending(x => x.InstrumentCode).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.InstrumentCode).ThenBy(x => x.Id),
            "instrumentName" => descending ? queryable.OrderByDescending(x => x.InstrumentName).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.InstrumentName).ThenBy(x => x.Id),
            "classificationType" => descending ? queryable.OrderByDescending(x => x.ClassificationType).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.ClassificationType).ThenBy(x => x.Id),
            "currency" => descending ? queryable.OrderByDescending(x => x.Currency).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.Currency).ThenBy(x => x.Id),
            "taxPeriod" => descending ? queryable.OrderByDescending(x => x.TaxPeriod).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.TaxPeriod).ThenBy(x => x.Id),
            "validTo" => descending ? queryable.OrderByDescending(x => x.ValidTo).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.ValidTo).ThenBy(x => x.Id),
            "status" => descending ? queryable.OrderByDescending(x => x.Status).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.Status).ThenBy(x => x.Id),
            "createdAt" => descending ? queryable.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
            "updatedAt" => descending ? queryable.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.UpdatedAt).ThenBy(x => x.Id),
            _ => descending ? queryable.OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id) : queryable.OrderBy(x => x.ValidFrom).ThenBy(x => x.Id)
        };
    }
}

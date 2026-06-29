using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.BackupMetadata;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.BackupMetadata;

public sealed class BackupMetadataQueryService(NuamExchangeDbContext dbContext) : IBackupMetadataQueryService
{
    public async Task<PagedResult<BackupMetadataListItemDto>> GetAsync(ValidatedBackupMetadataQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(dbContext.BackupRecords.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await ApplyOrdering(source, query.SortBy, query.SortDirection)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new BackupMetadataListItemDto(x.Id, x.BackupType, x.BackupStatus, x.BackupAt, x.Observation != null && x.Observation != string.Empty))
            .ToListAsync(cancellationToken);
        return new PagedResult<BackupMetadataListItemDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<BackupMetadataDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => dbContext.BackupRecords.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BackupMetadataDetailDto(x.Id, x.BackupType, x.BackupStatus, x.BackupAt, x.Observation != null && x.Observation != string.Empty))
            .SingleOrDefaultAsync(cancellationToken);

    private static IQueryable<BackupRecord> ApplyFilters(IQueryable<BackupRecord> query, ValidatedBackupMetadataQuery filter)
    {
        if (filter.BackupType is not null) query = query.Where(x => x.BackupType == filter.BackupType);
        if (filter.Status is not null) query = query.Where(x => x.BackupStatus == filter.Status);
        if (filter.DateFrom is not null) query = query.Where(x => x.BackupAt >= filter.DateFrom);
        if (filter.DateTo is not null) query = query.Where(x => x.BackupAt <= filter.DateTo);
        return query;
    }

    private static IOrderedQueryable<BackupRecord> ApplyOrdering(IQueryable<BackupRecord> query, string sortBy, string direction)
    {
        var desc = direction == "desc";
        return sortBy switch
        {
            "id" => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
            "backupType" => desc ? query.OrderByDescending(x => x.BackupType).ThenByDescending(x => x.BackupAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.BackupType).ThenBy(x => x.BackupAt).ThenBy(x => x.Id),
            "status" => desc ? query.OrderByDescending(x => x.BackupStatus).ThenByDescending(x => x.BackupAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.BackupStatus).ThenBy(x => x.BackupAt).ThenBy(x => x.Id),
            _ => desc ? query.OrderByDescending(x => x.BackupAt).ThenByDescending(x => x.Id) : query.OrderBy(x => x.BackupAt).ThenBy(x => x.Id)
        };
    }
}

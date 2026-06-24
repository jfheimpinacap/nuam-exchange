using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxClassifications;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxClassifications;

public sealed class BulkLoadQueryService(NuamExchangeDbContext dbContext) : IBulkLoadQueryService
{
    public async Task<PagedResult<BulkLoadSummaryDto>> GetAsync(ValidatedBulkLoadQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(dbContext.UploadFiles.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await ApplyOrdering(source, query.SortBy, query.SortDirection)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new BulkLoadSummaryDto(x.Id, x.UploadType, x.FileName, x.Extension, x.FileSizeBytes, x.UploadStatus, x.TotalRecords, x.ValidRecords, x.ErrorRecords, x.Observation, x.UploadedAt,
                new BulkLoadTemplateDto(x.UploadTemplate.Id, x.UploadTemplate.UploadType, x.UploadTemplate.TemplateName, x.UploadTemplate.TemplateVersion, x.UploadTemplate.AllowedFormat, x.UploadTemplate.IsActive),
                x.Details.Count, x.Errors.Count))
            .ToListAsync(cancellationToken);
        return new PagedResult<BulkLoadSummaryDto>(items, query.Page, query.PageSize, totalCount);
    }

    public Task<BulkLoadSummaryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => dbContext.UploadFiles.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new BulkLoadSummaryDto(x.Id, x.UploadType, x.FileName, x.Extension, x.FileSizeBytes, x.UploadStatus, x.TotalRecords, x.ValidRecords, x.ErrorRecords, x.Observation, x.UploadedAt,
                new BulkLoadTemplateDto(x.UploadTemplate.Id, x.UploadTemplate.UploadType, x.UploadTemplate.TemplateName, x.UploadTemplate.TemplateVersion, x.UploadTemplate.AllowedFormat, x.UploadTemplate.IsActive),
                x.Details.Count, x.Errors.Count))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<PagedResult<BulkLoadDetailDto>?> GetDetailsAsync(int id, ValidatedBulkLoadDetailQuery query, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.UploadFiles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken)) return null;
        var source = dbContext.BulkUploadDetails.AsNoTracking().Where(x => x.UploadFileId == id);
        if (query.Status is not null) source = source.Where(x => x.RowStatus == query.Status);
        if (query.AffectedField is not null) source = source.Where(x => x.AffectedField == query.AffectedField);
        if (query.TaxClassificationId is not null) source = source.Where(x => x.TaxClassificationId == query.TaxClassificationId);
        if (query.RowNumber is not null) source = source.Where(x => x.RowNumber == query.RowNumber);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source.OrderBy(x => x.RowNumber).ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new BulkLoadDetailDto(x.Id, x.UploadFileId, x.TaxClassificationId, x.RowNumber, x.AffectedField, x.FactorValue, x.AmountValue, x.OriginalTextValue, x.RowStatus, x.Observation, x.CreatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<BulkLoadDetailDto>(items, query.Page, query.PageSize, totalCount);
    }

    public async Task<PagedResult<BulkLoadErrorDto>?> GetErrorsAsync(int id, ValidatedBulkLoadErrorQuery query, CancellationToken cancellationToken = default)
    {
        if (!await dbContext.UploadFiles.AsNoTracking().AnyAsync(x => x.Id == id, cancellationToken)) return null;
        var source = dbContext.BulkUploadErrors.AsNoTracking().Where(x => x.UploadFileId == id);
        if (query.Severity is not null) source = source.Where(x => x.Severity == query.Severity);
        if (query.Column is not null) source = source.Where(x => x.ColumnName == query.Column);
        if (query.RowNumber is not null) source = source.Where(x => x.RowNumber == query.RowNumber);
        var totalCount = await source.CountAsync(cancellationToken);
        var items = await source.OrderBy(x => x.RowNumber).ThenBy(x => x.CreatedAt).ThenBy(x => x.Id).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new BulkLoadErrorDto(x.Id, x.UploadFileId, x.RowNumber, x.ColumnName, x.ErrorDescription, x.Severity, x.CreatedAt))
            .ToListAsync(cancellationToken);
        return new PagedResult<BulkLoadErrorDto>(items, query.Page, query.PageSize, totalCount);
    }

    private static IQueryable<UploadFile> ApplyFilters(IQueryable<UploadFile> q, ValidatedBulkLoadQuery query)
    {
        if (query.UploadType is not null) q = q.Where(x => x.UploadType == query.UploadType);
        if (query.Status is not null) q = q.Where(x => x.UploadStatus == query.Status);
        if (query.DateFrom is not null) q = q.Where(x => x.UploadedAt >= query.DateFrom);
        if (query.DateTo is not null) q = q.Where(x => x.UploadedAt <= query.DateTo);
        return q;
    }
    private static IOrderedQueryable<UploadFile> ApplyOrdering(IQueryable<UploadFile> q, string sortBy, string dir)
    {
        var desc = dir == "desc";
        return sortBy switch
        {
            "id" => desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id),
            "uploadType" => desc ? q.OrderByDescending(x => x.UploadType).ThenByDescending(x => x.Id) : q.OrderBy(x => x.UploadType).ThenBy(x => x.Id),
            "status" => desc ? q.OrderByDescending(x => x.UploadStatus).ThenByDescending(x => x.Id) : q.OrderBy(x => x.UploadStatus).ThenBy(x => x.Id),
            "fileName" => desc ? q.OrderByDescending(x => x.FileName).ThenByDescending(x => x.Id) : q.OrderBy(x => x.FileName).ThenBy(x => x.Id),
            "totalRecords" => desc ? q.OrderByDescending(x => x.TotalRecords).ThenByDescending(x => x.Id) : q.OrderBy(x => x.TotalRecords).ThenBy(x => x.Id),
            "validRecords" => desc ? q.OrderByDescending(x => x.ValidRecords).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ValidRecords).ThenBy(x => x.Id),
            "errorRecords" => desc ? q.OrderByDescending(x => x.ErrorRecords).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ErrorRecords).ThenBy(x => x.Id),
            _ => desc ? q.OrderByDescending(x => x.UploadedAt).ThenByDescending(x => x.Id) : q.OrderBy(x => x.UploadedAt).ThenBy(x => x.Id)
        };
    }
}

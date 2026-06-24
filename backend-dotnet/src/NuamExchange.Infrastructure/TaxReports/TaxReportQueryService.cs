using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NuamExchange.Application.TaxReports;
using NuamExchange.Domain.Entities;
using NuamExchange.Infrastructure.Persistence;

namespace NuamExchange.Infrastructure.TaxReports;

public sealed class TaxReportQueryService(NuamExchangeDbContext dbContext) : ITaxReportQueryService
{
    private const string CsvContentType = "text/csv; charset=utf-8";

    public async Task<TaxClassificationReportDto> GetTaxClassificationsAsync(ValidatedTaxClassificationReportQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(dbContext.TaxClassifications.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        var summary = await BuildSummaryAsync(source, totalCount, cancellationToken);
        var items = await ApplyOrdering(source, query.SortBy, query.SortDirection).Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).Select(ToItem()).ToListAsync(cancellationToken);
        var totalPages = totalCount == 0 ? 0 : (int)Math.Ceiling(totalCount / (double)query.PageSize);
        return new TaxClassificationReportDto(DateTime.UtcNow, ToFilters(query), summary, items, query.Page, query.PageSize, totalCount, totalPages);
    }

    public async Task<TaxClassificationCsvExportDto> ExportTaxClassificationsAsync(ValidatedTaxClassificationReportQuery query, CancellationToken cancellationToken = default)
    {
        var source = ApplyFilters(dbContext.TaxClassifications.AsNoTracking(), query);
        var totalCount = await source.CountAsync(cancellationToken);
        if (totalCount > TaxClassificationReportDefaults.MaxExportRows) throw new InvalidOperationException($"La exportación supera el límite permitido de {TaxClassificationReportDefaults.MaxExportRows} filas.");
        var rows = await ApplyOrdering(source, query.SortBy, query.SortDirection).Select(ToItem()).ToListAsync(cancellationToken);
        var sb = new StringBuilder();
        sb.AppendLine("market;instrumentCode;instrumentName;classificationType;taxPeriod;status;appliedFactor;referenceAmount;currency;validFrom;validTo;updatedAt");
        foreach (var row in rows)
        {
            sb.AppendJoin(';', Safe(row.Market), Safe(row.InstrumentCode), Safe(row.InstrumentName), Safe(row.ClassificationType), row.TaxPeriod.ToString(CultureInfo.InvariantCulture), Safe(row.Status), Decimal(row.AppliedFactor), Decimal(row.ReferenceAmount), Safe(row.Currency), row.ValidFrom.ToString("O", CultureInfo.InvariantCulture), row.ValidTo?.ToString("O", CultureInfo.InvariantCulture) ?? string.Empty, row.UpdatedAt.ToString("O", CultureInfo.InvariantCulture));
            sb.AppendLine();
        }
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
        return new TaxClassificationCsvExportDto(bytes, $"reporte_calificaciones_tributarias_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv", CsvContentType);
    }

    private static IQueryable<TaxClassification> ApplyFilters(IQueryable<TaxClassification> q, ValidatedTaxClassificationReportQuery query)
    {
        if (query.Market is not null) q = q.Where(x => x.Market == query.Market);
        if (query.InstrumentCode is not null) q = q.Where(x => x.InstrumentCode == query.InstrumentCode);
        if (query.TaxPeriod is not null) q = q.Where(x => x.TaxPeriod == query.TaxPeriod.Value);
        if (query.Status is not null) q = q.Where(x => x.Status == query.Status);
        if (query.ClassificationType is not null) q = q.Where(x => x.ClassificationType == query.ClassificationType);
        if (query.Currency is not null) q = q.Where(x => x.Currency == query.Currency);
        return q;
    }

    private static IOrderedQueryable<TaxClassification> ApplyOrdering(IQueryable<TaxClassification> q, string sortBy, string sortDirection)
    {
        var desc = sortDirection == "desc";
        return sortBy switch
        {
            "id" => desc ? q.OrderByDescending(x => x.Id) : q.OrderBy(x => x.Id),
            "market" => desc ? q.OrderByDescending(x => x.Market).ThenByDescending(x => x.Id) : q.OrderBy(x => x.Market).ThenBy(x => x.Id),
            "instrumentCode" => desc ? q.OrderByDescending(x => x.InstrumentCode).ThenByDescending(x => x.Id) : q.OrderBy(x => x.InstrumentCode).ThenBy(x => x.Id),
            "instrumentName" => desc ? q.OrderByDescending(x => x.InstrumentName).ThenByDescending(x => x.Id) : q.OrderBy(x => x.InstrumentName).ThenBy(x => x.Id),
            "classificationType" => desc ? q.OrderByDescending(x => x.ClassificationType).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ClassificationType).ThenBy(x => x.Id),
            "status" => desc ? q.OrderByDescending(x => x.Status).ThenByDescending(x => x.Id) : q.OrderBy(x => x.Status).ThenBy(x => x.Id),
            "appliedFactor" => desc ? q.OrderByDescending(x => x.AppliedFactor).ThenByDescending(x => x.Id) : q.OrderBy(x => x.AppliedFactor).ThenBy(x => x.Id),
            "referenceAmount" => desc ? q.OrderByDescending(x => x.ReferenceAmount).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ReferenceAmount).ThenBy(x => x.Id),
            "currency" => desc ? q.OrderByDescending(x => x.Currency).ThenByDescending(x => x.Id) : q.OrderBy(x => x.Currency).ThenBy(x => x.Id),
            "validFrom" => desc ? q.OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ValidFrom).ThenBy(x => x.Id),
            "validTo" => desc ? q.OrderByDescending(x => x.ValidTo).ThenByDescending(x => x.Id) : q.OrderBy(x => x.ValidTo).ThenBy(x => x.Id),
            "updatedAt" => desc ? q.OrderByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id) : q.OrderBy(x => x.UpdatedAt).ThenBy(x => x.Id),
            _ => desc ? q.OrderByDescending(x => x.TaxPeriod).ThenByDescending(x => x.UpdatedAt).ThenByDescending(x => x.Id) : q.OrderBy(x => x.TaxPeriod).ThenBy(x => x.UpdatedAt).ThenBy(x => x.Id)
        };
    }

    private static System.Linq.Expressions.Expression<Func<TaxClassification, TaxClassificationReportItemDto>> ToItem() => x => new TaxClassificationReportItemDto(x.Id, x.Market, x.InstrumentCode, x.InstrumentName, x.ClassificationType, x.TaxPeriod, x.Status, x.AppliedFactor, x.ReferenceAmount, x.Currency, x.ValidFrom, x.ValidTo, x.UpdatedAt);
    private static TaxClassificationReportAppliedFiltersDto ToFilters(ValidatedTaxClassificationReportQuery q) => new(q.Market, q.InstrumentCode, q.TaxPeriod, q.Status, q.ClassificationType, q.Currency, q.SortBy, q.SortDirection);
    private static async Task<TaxClassificationReportSummaryDto> BuildSummaryAsync(IQueryable<TaxClassification> source, int total, CancellationToken ct) => new(total, await source.GroupBy(x => x.Status).Select(g => new TaxClassificationReportCountDto(g.Key, g.Count())).ToListAsync(ct), await source.GroupBy(x => x.ClassificationType).Select(g => new TaxClassificationReportCountDto(g.Key, g.Count())).ToListAsync(ct), await source.CountAsync(x => x.AppliedFactor != null, ct), await source.CountAsync(x => x.ReferenceAmount != null, ct), await source.Where(x => x.ReferenceAmount != null).GroupBy(x => x.Currency).Select(g => new TaxClassificationReportCurrencyTotalDto(g.Key, g.Count(), g.Sum(x => x.ReferenceAmount)!.Value)).ToListAsync(ct));
    private static string Decimal(decimal? value) => value?.ToString("0.############################", CultureInfo.InvariantCulture) ?? string.Empty;
    private static string Safe(string? value) { if (value is null) return string.Empty; var sanitized = value.Length > 0 && "=+-@".Contains(value[0]) ? "'" + value : value; return sanitized.IndexOfAny([';', '"', '\r', '\n']) >= 0 ? "\"" + sanitized.Replace("\"", "\"\"") + "\"" : sanitized; }
}

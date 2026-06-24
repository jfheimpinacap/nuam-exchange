namespace NuamExchange.Application.TaxClassifications;

public interface ITaxClassificationQueryService
{
    Task<PagedResult<TaxClassificationListItemDto>> GetAsync(ValidatedTaxClassificationQuery query, CancellationToken cancellationToken = default);
    Task<TaxClassificationDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TaxClassificationFilterOptionsDto> GetFilterOptionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TaxClassificationHistoryDto>?> GetHistoryAsync(int id, CancellationToken cancellationToken = default);
}

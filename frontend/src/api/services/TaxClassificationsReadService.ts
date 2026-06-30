import type { TaxClassificationDetailDto, TaxClassificationFilterOptionsDto, TaxClassificationListRequestDto, TaxClassificationListResponseDto } from '../contracts/taxClassificationsRead';

export interface TaxClassificationsReadService {
  list(request: TaxClassificationListRequestDto, signal?: AbortSignal): Promise<TaxClassificationListResponseDto>;
  getFilterOptions(signal?: AbortSignal): Promise<TaxClassificationFilterOptionsDto>;
  getById(id: number, signal?: AbortSignal): Promise<TaxClassificationDetailDto>;
}

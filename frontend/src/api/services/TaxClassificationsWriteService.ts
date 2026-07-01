import type { TaxClassificationDetailDto } from '../contracts/taxClassificationsRead';
import type { TaxClassificationWriteRequestDto } from '../contracts/taxClassificationsWrite';

export interface TaxClassificationsWriteService {
  create(request: TaxClassificationWriteRequestDto, signal?: AbortSignal): Promise<TaxClassificationDetailDto>;
  update(id: number, request: TaxClassificationWriteRequestDto, signal?: AbortSignal): Promise<TaxClassificationDetailDto>;
  copy(id: number, signal?: AbortSignal): Promise<TaxClassificationDetailDto>;
}

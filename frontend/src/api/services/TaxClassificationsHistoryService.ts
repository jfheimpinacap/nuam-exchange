import type { TaxClassificationHistoryDto } from '../contracts/taxClassificationsHistory';

export interface TaxClassificationsHistoryService {
  getHistory(id: number, signal?: AbortSignal): Promise<TaxClassificationHistoryDto[]>;
}

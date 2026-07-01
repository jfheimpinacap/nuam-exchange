import type { HttpClient } from '../client/HttpClient';
import type { TaxClassificationDetailDto } from '../contracts/taxClassificationsRead';
import type { TaxClassificationWriteRequestDto } from '../contracts/taxClassificationsWrite';
import { parseTaxClassificationDetail } from './HttpTaxClassificationsReadService';
import type { TaxClassificationsWriteService } from './TaxClassificationsWriteService';

export class HttpTaxClassificationsWriteService implements TaxClassificationsWriteService {
  constructor(private readonly http: HttpClient) {}

  async create(request: TaxClassificationWriteRequestDto, signal?: AbortSignal): Promise<TaxClassificationDetailDto> {
    return parseTaxClassificationDetail(await this.http.post<unknown>('/tax-classifications', request, { signal }));
  }

  async update(id: number, request: TaxClassificationWriteRequestDto, signal?: AbortSignal): Promise<TaxClassificationDetailDto> {
    return parseTaxClassificationDetail(await this.http.put<unknown>(`/tax-classifications/${encodeURIComponent(String(id))}`, request, { signal }));
  }

  async copy(id: number, signal?: AbortSignal): Promise<TaxClassificationDetailDto> {
    return parseTaxClassificationDetail(await this.http.post<unknown>(`/tax-classifications/${encodeURIComponent(String(id))}/copy`, undefined, { signal }));
  }
}

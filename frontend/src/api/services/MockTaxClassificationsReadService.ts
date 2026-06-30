import { mockClassifications } from '../../mocks/classifications';
import type { TaxClassificationDetailDto, TaxClassificationFilterOptionsDto, TaxClassificationListRequestDto, TaxClassificationListResponseDto, TaxClassificationReadDto } from '../contracts/taxClassificationsRead';
import type { TaxClassificationsReadService } from './TaxClassificationsReadService';

function normalizeStatus(status: string) { return status.toUpperCase(); }
function toReadDto(index: number): TaxClassificationReadDto {
  const item = mockClassifications[index];
  return { id: index + 1, market: item.mercado, instrumentCode: item.instrumento, instrumentName: item.instrumento, classificationType: item.origen, description: item.descripcion, updatePercentage: null, appliedFactor: item.factorActualizacion, referenceAmount: item.monto, currency: 'CLP', taxPeriod: item.ejercicio, validFrom: item.fechaPago.split('-').reverse().join('-'), validTo: null, status: normalizeStatus(item.estado) };
}
const readItems = mockClassifications.map((_, index) => toReadDto(index));
function sortValue(item: TaxClassificationReadDto, key: string): string | number { return key in item ? String(item[key as keyof TaxClassificationReadDto] ?? '') : ''; }

export class MockTaxClassificationsReadService implements TaxClassificationsReadService {
  async list(request: TaxClassificationListRequestDto): Promise<TaxClassificationListResponseDto> {
    const search = request.search?.trim().toLowerCase();
    const filtered = readItems.filter((item) => {
      const matchesSearch = !search || [item.instrumentCode, item.instrumentName, item.description, item.classificationType, item.market, item.status].some((value) => value?.toLowerCase().includes(search));
      return matchesSearch && (!request.market || item.market === request.market) && (!request.exercise || item.taxPeriod === request.exercise) && (!request.status || item.status === request.status);
    });
    const direction = request.sortDirection === 'asc' ? 1 : -1;
    const sorted = [...filtered].sort((a, b) => String(sortValue(a, request.sortBy)).localeCompare(String(sortValue(b, request.sortBy)), 'es') * direction);
    const start = (request.page - 1) * request.pageSize;
    return { items: sorted.slice(start, start + request.pageSize), page: request.page, pageSize: request.pageSize, totalCount: filtered.length, totalPages: Math.ceil(filtered.length / request.pageSize) };
  }
  async getFilterOptions(): Promise<TaxClassificationFilterOptionsDto> { return { markets: [...new Set(readItems.map((item) => item.market))], exercises: [...new Set(readItems.map((item) => item.taxPeriod))].sort(), statuses: [...new Set(readItems.map((item) => item.status))] }; }
  async getById(id: number): Promise<TaxClassificationDetailDto> { const item = readItems.find((record) => record.id === id); if (!item) throw new Error('No encontrado.'); return { ...item, createdAt: item.validFrom, updatedAt: item.validFrom }; }
}

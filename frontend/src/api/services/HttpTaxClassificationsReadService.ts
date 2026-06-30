import { ApiError } from '../client/ApiError';
import type { HttpClient } from '../client/HttpClient';
import type { TaxClassificationDetailDto, TaxClassificationFilterOptionsDto, TaxClassificationListRequestDto, TaxClassificationListResponseDto, TaxClassificationReadDto } from '../contracts/taxClassificationsRead';
import type { TaxClassificationsReadService } from './TaxClassificationsReadService';

function isRecord(value: unknown): value is Record<string, unknown> { return Boolean(value && typeof value === 'object' && !Array.isArray(value)); }
function isStringOrNull(value: unknown): value is string | null { return typeof value === 'string' || value === null; }
function isNumberOrNull(value: unknown): value is number | null { return typeof value === 'number' || value === null; }
function invalid(message: string) { return new ApiError({ code: 'INVALID_RESPONSE', message }); }
function stringArray(value: unknown): string[] | null { return Array.isArray(value) && value.every((item) => typeof item === 'string') ? value : null; }
function numberArray(value: unknown): number[] | null { return Array.isArray(value) && value.every((item) => typeof item === 'number') ? value : null; }

function parseReadItem(value: unknown): TaxClassificationReadDto {
  if (!isRecord(value)) throw invalid('Respuesta inválida de calificaciones tributarias.');
  const { id, market, instrumentCode, instrumentName, classificationType, description, updatePercentage, appliedFactor, referenceAmount, currency, taxPeriod, validFrom, validTo, status } = value;
  if (typeof id !== 'number' || typeof market !== 'string' || !isStringOrNull(instrumentCode) || !isStringOrNull(instrumentName) || typeof classificationType !== 'string' || !isStringOrNull(description) || !isNumberOrNull(updatePercentage) || !isNumberOrNull(appliedFactor) || !isNumberOrNull(referenceAmount) || typeof currency !== 'string' || typeof taxPeriod !== 'number' || typeof validFrom !== 'string' || !isStringOrNull(validTo) || typeof status !== 'string') {
    throw invalid('Respuesta inválida de calificaciones tributarias.');
  }
  return { id, market, instrumentCode, instrumentName, classificationType, description, updatePercentage, appliedFactor, referenceAmount, currency, taxPeriod, validFrom, validTo, status };
}

function parseDetail(value: unknown): TaxClassificationDetailDto {
  const base = parseReadItem(value);
  if (!isRecord(value)) throw invalid('Respuesta inválida del detalle de calificación tributaria.');
  const { createdAt, updatedAt, creatorUserId } = value;
  if (createdAt !== undefined && typeof createdAt !== 'string') throw invalid('Respuesta inválida del detalle de calificación tributaria.');
  if (updatedAt !== undefined && typeof updatedAt !== 'string') throw invalid('Respuesta inválida del detalle de calificación tributaria.');
  if (creatorUserId !== undefined && typeof creatorUserId !== 'string' && typeof creatorUserId !== 'number' && creatorUserId !== null) throw invalid('Respuesta inválida del detalle de calificación tributaria.');
  return { ...base, createdAt, updatedAt, creatorUserId };
}

function parseList(value: unknown): TaxClassificationListResponseDto {
  if (!isRecord(value) || !Array.isArray(value.items) || typeof value.page !== 'number' || typeof value.pageSize !== 'number' || typeof value.totalCount !== 'number' || typeof value.totalPages !== 'number') throw invalid('Respuesta inválida del listado de calificaciones tributarias.');
  return { items: value.items.map(parseReadItem), page: value.page, pageSize: value.pageSize, totalCount: value.totalCount, totalPages: value.totalPages };
}

function parseOptions(value: unknown): TaxClassificationFilterOptionsDto {
  if (!isRecord(value)) throw invalid('Respuesta inválida de filtros de calificaciones tributarias.');
  const markets = stringArray(value.markets); const exercises = numberArray(value.exercises); const statuses = stringArray(value.statuses);
  if (!markets || !exercises || !statuses) throw invalid('Respuesta inválida de filtros de calificaciones tributarias.');
  return { markets, exercises, statuses };
}

export class HttpTaxClassificationsReadService implements TaxClassificationsReadService {
  constructor(private readonly http: HttpClient) {}
  async list(request: TaxClassificationListRequestDto, signal?: AbortSignal) { return parseList(await this.http.get<unknown>('/tax-classifications', { query: { ...request }, signal })); }
  async getFilterOptions(signal?: AbortSignal) { return parseOptions(await this.http.get<unknown>('/tax-classifications/filter-options', { signal })); }
  async getById(id: number, signal?: AbortSignal) { return parseDetail(await this.http.get<unknown>(`/tax-classifications/${encodeURIComponent(String(id))}`, { signal })); }
}

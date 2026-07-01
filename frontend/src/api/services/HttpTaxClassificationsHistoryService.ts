import { ApiError } from '../client/ApiError';
import type { HttpClient } from '../client/HttpClient';
import type { TaxClassificationHistoryDto } from '../contracts/taxClassificationsHistory';
import type { TaxClassificationsHistoryService } from './TaxClassificationsHistoryService';

function isRecord(value: unknown): value is Record<string, unknown> {
  return Boolean(value && typeof value === 'object' && !Array.isArray(value));
}

function isStringOrNull(value: unknown): value is string | null {
  return typeof value === 'string' || value === null;
}

function invalid(message: string) {
  return new ApiError({ code: 'INVALID_RESPONSE', message });
}

function parseHistoryItem(value: unknown): TaxClassificationHistoryDto {
  if (!isRecord(value)) throw invalid('Respuesta inválida del historial de calificación tributaria.');
  const { id, taxClassificationId, userId, changeType, modifiedField, previousValue, newValue, observation, changedAt } = value;
  if (typeof id !== 'number' || typeof taxClassificationId !== 'number' || typeof userId !== 'number' || typeof changeType !== 'string' || !isStringOrNull(modifiedField) || !isStringOrNull(previousValue) || !isStringOrNull(newValue) || !isStringOrNull(observation) || typeof changedAt !== 'string') {
    throw invalid('Respuesta inválida del historial de calificación tributaria.');
  }
  return { id, taxClassificationId, userId, changeType, modifiedField, previousValue, newValue, observation, changedAt };
}

function parseHistory(value: unknown): TaxClassificationHistoryDto[] {
  if (!Array.isArray(value)) throw invalid('Respuesta inválida del historial de calificación tributaria.');
  return value.map(parseHistoryItem);
}

export class HttpTaxClassificationsHistoryService implements TaxClassificationsHistoryService {
  constructor(private readonly http: HttpClient) {}

  async getHistory(id: number, signal?: AbortSignal) {
    return parseHistory(await this.http.get<unknown>(`/tax-classifications/${encodeURIComponent(String(id))}/history`, { signal }));
  }
}

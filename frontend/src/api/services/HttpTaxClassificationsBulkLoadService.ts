import type { HttpClient } from '../client/HttpClient';
import type { BulkLoadXAmountErrorDto, BulkLoadXAmountResultDto, BulkLoadXFactorErrorDto, BulkLoadXFactorResultDto, TaxClassificationsBulkLoadService } from './TaxClassificationsBulkLoadService';

function isRecord(value: unknown): value is Record<string, unknown> {
  return Boolean(value && typeof value === 'object');
}

function parseNumber(value: unknown, field: string, label: string): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    throw new Error(`Respuesta inválida de carga ${label}: ${field}.`);
  }
  return value;
}

function parseString(value: unknown, field: string, label: string): string {
  if (typeof value !== 'string') {
    throw new Error(`Respuesta inválida de carga ${label}: ${field}.`);
  }
  return value;
}

function parseErrors<TError extends BulkLoadXFactorErrorDto | BulkLoadXAmountErrorDto>(value: unknown, label: string): TError[] {
  if (!Array.isArray(value)) {
    throw new Error(`Respuesta inválida de carga ${label}: errors.`);
  }
  return value.map((item) => {
    if (!isRecord(item)) {
      throw new Error(`Respuesta inválida de carga ${label}: errors.`);
    }
    return {
      rowNumber: parseNumber(item.rowNumber, 'errors.rowNumber', label),
      code: parseString(item.code, 'errors.code', label),
      message: parseString(item.message, 'errors.message', label),
    } as TError;
  });
}

function parseUpdatedIds(value: unknown, label: string): number[] {
  if (!Array.isArray(value)) {
    throw new Error(`Respuesta inválida de carga ${label}: updatedTaxClassificationIds.`);
  }
  return value.map((item) => parseNumber(item, 'updatedTaxClassificationIds', label));
}

function parseBulkLoadResult<TError extends BulkLoadXFactorErrorDto | BulkLoadXAmountErrorDto>(value: unknown, label: string) {
  if (!isRecord(value)) {
    throw new Error(`Respuesta inválida de carga ${label}.`);
  }
  return {
    uploadId: parseNumber(value.uploadId, 'uploadId', label),
    totalRows: parseNumber(value.totalRows, 'totalRows', label),
    successfulRows: parseNumber(value.successfulRows, 'successfulRows', label),
    failedRows: parseNumber(value.failedRows, 'failedRows', label),
    updatedTaxClassificationIds: parseUpdatedIds(value.updatedTaxClassificationIds, label),
    errors: parseErrors<TError>(value.errors, label),
  };
}

function parseBulkLoadXFactorResult(value: unknown): BulkLoadXFactorResultDto {
  return parseBulkLoadResult<BulkLoadXFactorErrorDto>(value, 'X Factor');
}

function parseBulkLoadXAmountResult(value: unknown): BulkLoadXAmountResultDto {
  return parseBulkLoadResult<BulkLoadXAmountErrorDto>(value, 'X Monto');
}

export class HttpTaxClassificationsBulkLoadService implements TaxClassificationsBulkLoadService {
  constructor(private readonly http: HttpClient) {}

  async uploadXFactor(file: File, signal?: AbortSignal): Promise<BulkLoadXFactorResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    const payload = await this.http.post<unknown>('/tax-classifications/bulk-loads/x-factor', formData, { signal });
    return parseBulkLoadXFactorResult(payload);
  }

  async uploadXAmount(file: File, signal?: AbortSignal): Promise<BulkLoadXAmountResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    const payload = await this.http.post<unknown>('/tax-classifications/bulk-loads/x-amount', formData, { signal });
    return parseBulkLoadXAmountResult(payload);
  }
}

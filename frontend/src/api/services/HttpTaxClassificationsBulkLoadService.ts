import type { HttpClient } from '../client/HttpClient';
import type { BulkLoadXFactorErrorDto, BulkLoadXFactorResultDto, TaxClassificationsBulkLoadService } from './TaxClassificationsBulkLoadService';

function isRecord(value: unknown): value is Record<string, unknown> {
  return Boolean(value && typeof value === 'object');
}

function parseNumber(value: unknown, field: string): number {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    throw new Error(`Respuesta inválida de carga X Factor: ${field}.`);
  }
  return value;
}

function parseString(value: unknown, field: string): string {
  if (typeof value !== 'string') {
    throw new Error(`Respuesta inválida de carga X Factor: ${field}.`);
  }
  return value;
}

function parseErrors(value: unknown): BulkLoadXFactorErrorDto[] {
  if (!Array.isArray(value)) {
    throw new Error('Respuesta inválida de carga X Factor: errors.');
  }
  return value.map((item) => {
    if (!isRecord(item)) {
      throw new Error('Respuesta inválida de carga X Factor: errors.');
    }
    return {
      rowNumber: parseNumber(item.rowNumber, 'errors.rowNumber'),
      code: parseString(item.code, 'errors.code'),
      message: parseString(item.message, 'errors.message'),
    };
  });
}

function parseUpdatedIds(value: unknown): number[] {
  if (!Array.isArray(value)) {
    throw new Error('Respuesta inválida de carga X Factor: updatedTaxClassificationIds.');
  }
  return value.map((item) => parseNumber(item, 'updatedTaxClassificationIds'));
}

function parseBulkLoadXFactorResult(value: unknown): BulkLoadXFactorResultDto {
  if (!isRecord(value)) {
    throw new Error('Respuesta inválida de carga X Factor.');
  }
  return {
    uploadId: parseNumber(value.uploadId, 'uploadId'),
    totalRows: parseNumber(value.totalRows, 'totalRows'),
    successfulRows: parseNumber(value.successfulRows, 'successfulRows'),
    failedRows: parseNumber(value.failedRows, 'failedRows'),
    updatedTaxClassificationIds: parseUpdatedIds(value.updatedTaxClassificationIds),
    errors: parseErrors(value.errors),
  };
}

export class HttpTaxClassificationsBulkLoadService implements TaxClassificationsBulkLoadService {
  constructor(private readonly http: HttpClient) {}

  async uploadXFactor(file: File, signal?: AbortSignal): Promise<BulkLoadXFactorResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    const payload = await this.http.post<unknown>('/tax-classifications/bulk-loads/x-factor', formData, { signal });
    return parseBulkLoadXFactorResult(payload);
  }
}

import type { HttpClient } from '../client/HttpClient';
import type { DocumentReviewService, PdfDocumentReviewResultDto, PdfReviewStatus } from './DocumentReviewService';

const statuses = new Set(['VALID', 'INCOMPLETE', 'UNSUPPORTED', 'INVALID_FILE']);
function isRecord(value: unknown): value is Record<string, unknown> { return Boolean(value && typeof value === 'object'); }
function parseStringArray(value: unknown): string[] { return Array.isArray(value) ? value.filter((x): x is string => typeof x === 'string') : []; }
function parseDetectedFields(value: unknown): Record<string, string> {
  if (!isRecord(value)) return {};
  return Object.fromEntries(Object.entries(value).filter((entry): entry is [string, string] => typeof entry[1] === 'string'));
}
function parseResult(value: unknown): PdfDocumentReviewResultDto {
  if (!isRecord(value)) throw new Error('Respuesta inválida de revisión PDF.');
  const status = typeof value.status === 'string' && statuses.has(value.status) ? value.status as PdfReviewStatus : 'INVALID_FILE';
  return {
    reviewId: typeof value.reviewId === 'string' ? value.reviewId : null,
    fileName: typeof value.fileName === 'string' ? value.fileName : '',
    fileSizeBytes: typeof value.fileSizeBytes === 'number' ? value.fileSizeBytes : 0,
    pageCount: typeof value.pageCount === 'number' ? value.pageCount : 0,
    status,
    message: typeof value.message === 'string' ? value.message : '',
    detectedFields: parseDetectedFields(value.detectedFields),
    missingFields: parseStringArray(value.missingFields),
    warnings: parseStringArray(value.warnings),
    textPreview: typeof value.textPreview === 'string' ? value.textPreview : '',
  };
}
export class HttpDocumentReviewService implements DocumentReviewService {
  constructor(private readonly http: HttpClient) {}
  async reviewPdf(file: File, signal?: AbortSignal): Promise<PdfDocumentReviewResultDto> {
    const formData = new FormData();
    formData.append('file', file);
    return parseResult(await this.http.post<unknown>('/document-reviews/pdf', formData, { signal }));
  }
}

type DataSource = 'mock' | 'api';

const DEFAULT_DATA_SOURCE: DataSource = 'mock';
const DEFAULT_BASE_URL = '/api';
const DEFAULT_TIMEOUT_MS = 10_000;

function parseDataSource(value: string | undefined): DataSource {
  const normalized = value?.trim();

  return normalized === 'mock' || normalized === 'api'
    ? normalized
    : DEFAULT_DATA_SOURCE;
}

function parseTimeout(value: string | undefined): number {
  const normalized = value?.trim();
  const parsed = Number(normalized);

  return Number.isInteger(parsed) && parsed > 0
    ? parsed
    : DEFAULT_TIMEOUT_MS;
}

function sanitizeBaseUrl(value: string | undefined): string {
  const normalized = value?.trim();

  if (!normalized) {
    return DEFAULT_BASE_URL;
  }

  const withoutQuery = normalized.split('?')[0];
  const withoutTrailingSlashes = withoutQuery.replace(/\/+$/, '');

  return withoutTrailingSlashes || '/';
}

const dataSource = parseDataSource(import.meta.env.VITE_DATA_SOURCE);
const baseUrl = sanitizeBaseUrl(import.meta.env.VITE_API_BASE_URL);
const timeoutMs = parseTimeout(import.meta.env.VITE_API_TIMEOUT_MS);

export const apiConfig = {
  dataSource,
  baseUrl,
  timeoutMs,
  isMock: dataSource === 'mock',
  isApi: dataSource === 'api',
} as const;

export type ApiConfig = typeof apiConfig;
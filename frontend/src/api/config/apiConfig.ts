import type { DataSourceMode } from '../../types';
export const apiConfig = { dataSource: (import.meta.env.VITE_DATA_SOURCE ?? 'mock') as DataSourceMode, baseUrl: import.meta.env.VITE_API_BASE_URL ?? '/api/v1', timeoutMs: Number(import.meta.env.VITE_API_TIMEOUT_MS ?? 10000) };
export const isMockMode = apiConfig.dataSource !== 'api';

export type DataSource = 'mock' | 'api';
export const dataSource = (import.meta.env.VITE_DATA_SOURCE ?? 'mock') as DataSource;
export const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';
export const apiTimeoutMs = Number(import.meta.env.VITE_API_TIMEOUT_MS ?? 10000);
export const isMockMode = dataSource !== 'api';

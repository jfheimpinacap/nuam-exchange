type DataSource = 'mock' | 'api';

function parseDataSource(value: string | undefined): DataSource {
  if (!value) return 'mock';
  if (value === 'mock' || value === 'api') return value;
  throw new Error(`Configuración inválida: VITE_DATA_SOURCE debe ser "mock" o "api". Valor recibido: "${value}".`);
}
function parseTimeout(value: string | undefined): number {
  const raw = value ?? '10000';
  const parsed = Number(raw);
  if (!Number.isInteger(parsed) || parsed <= 0) throw new Error('Configuración inválida: VITE_API_TIMEOUT_MS debe ser un entero positivo.');
  return parsed;
}
function sanitizeBaseUrl(value: string): string { return value.split('?')[0].replace(/\/+$/, ''); }
const dataSource = parseDataSource(import.meta.env.VITE_DATA_SOURCE);
const baseUrl = sanitizeBaseUrl(import.meta.env.VITE_API_BASE_URL ?? '/api/v1');
const timeoutMs = parseTimeout(import.meta.env.VITE_API_TIMEOUT_MS);
if (dataSource === 'api' && !baseUrl) throw new Error('Configuración inválida: VITE_API_BASE_URL es obligatoria en modo api.');
export const apiConfig = { dataSource, baseUrl, timeoutMs, isMock: dataSource === 'mock', isApi: dataSource === 'api' } as const;
export type ApiConfig = typeof apiConfig;

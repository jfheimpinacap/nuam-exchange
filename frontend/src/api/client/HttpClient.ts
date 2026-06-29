import { apiConfig } from '../config/apiConfig';
export class HttpClient {
  async get<T>(path: string, signal?: AbortSignal): Promise<T> {
    const controller = new AbortController();
    const timeout = window.setTimeout(() => controller.abort(), apiConfig.timeoutMs);
    signal?.addEventListener('abort', () => controller.abort(), { once: true });
    try { const response = await fetch(`${apiConfig.baseUrl}${path}`, { signal: controller.signal }); if (!response.ok) throw new Error(`HTTP ${response.status}`); return await response.json() as T; }
    finally { window.clearTimeout(timeout); }
  }
}
export const httpClient = new HttpClient();

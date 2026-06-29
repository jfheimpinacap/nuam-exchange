import { apiBaseUrl, apiTimeoutMs } from './config';

export async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  const controller = new AbortController();
  const timeout = window.setTimeout(() => controller.abort(), apiTimeoutMs);
  signal?.addEventListener('abort', () => controller.abort(), { once: true });
  try {
    const response = await fetch(`${apiBaseUrl}${path}`, { signal: controller.signal });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return (await response.json()) as T;
  } finally {
    window.clearTimeout(timeout);
  }
}

import { ApiError } from './ApiError';
import type { HttpClientOptions, RequestOptions } from './httpTypes';
import type { ApiErrorCode, ProblemDetailsDto } from '../contracts/common';

const jsonTypes = ['application/json', 'application/problem+json'];
function isProblemDetails(value: unknown): value is ProblemDetailsDto { return Boolean(value && typeof value === 'object' && 'title' in value && 'status' in value); }
function statusCodeToErrorCode(status: number): ApiErrorCode { if (status === 400 || status === 422) return 'VALIDATION_ERROR'; if (status === 401) return 'UNAUTHORIZED'; if (status === 403) return 'FORBIDDEN'; if (status === 404) return 'NOT_FOUND'; if (status === 409) return 'CONFLICT'; if (status >= 500) return 'SERVER_ERROR'; return 'INVALID_RESPONSE'; }
function createCorrelationId() { return globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`; }
function joinSignals(external: AbortSignal | undefined, timeoutMs: number) {
  const controller = new AbortController(); let timeoutTriggered = false;
  const timeout = window.setTimeout(() => { timeoutTriggered = true; controller.abort(); }, timeoutMs);
  const abort = () => controller.abort(); external?.addEventListener('abort', abort, { once: true });
  return { signal: controller.signal, timeoutTriggered: () => timeoutTriggered, cleanup: () => { window.clearTimeout(timeout); external?.removeEventListener('abort', abort); } };
}
export class HttpClient {
  constructor(private readonly options: HttpClientOptions) {}
  get<T>(path: string, options?: RequestOptions) { return this.request<T>('GET', path, options); }
  post<T>(path: string, body?: RequestOptions['body'], options?: Omit<RequestOptions, 'body'>) { return this.request<T>('POST', path, { ...options, body }); }
  put<T>(path: string, body?: RequestOptions['body'], options?: Omit<RequestOptions, 'body'>) { return this.request<T>('PUT', path, { ...options, body }); }
  patch<T>(path: string, body?: RequestOptions['body'], options?: Omit<RequestOptions, 'body'>) { return this.request<T>('PATCH', path, { ...options, body }); }
  delete<T>(path: string, options?: RequestOptions) { return this.request<T>('DELETE', path, options); }
  private buildUrl(path: string, query?: RequestOptions['query']) { const url = new URL(`${this.options.baseUrl.replace(/\/$/, '')}/${path.replace(/^\//, '')}`, window.location.origin); Object.entries(query ?? {}).forEach(([k, v]) => { if (v !== undefined && v !== null && v !== '') url.searchParams.set(k, String(v)); }); return url.pathname + url.search; }
  private async request<T>(method: string, path: string, requestOptions: RequestOptions = {}): Promise<T> {
    const signalState = joinSignals(requestOptions.signal, this.options.timeoutMs);
    try {
      const headers = new Headers(requestOptions.headers); headers.set('Accept', 'application/json, application/problem+json'); headers.set('x-correlation-id', createCorrelationId());
      const token = await this.options.getAccessToken?.(); if (token) headers.set('Authorization', `Bearer ${token}`);
      const init: RequestInit = { method, headers, signal: signalState.signal };
      if (requestOptions.body instanceof FormData) init.body = requestOptions.body;
      else if (requestOptions.body) { headers.set('Content-Type', 'application/json'); init.body = JSON.stringify(requestOptions.body); }
      const response = await fetch(this.buildUrl(path, requestOptions.query), init);
      if (response.status === 204) return undefined as T;
      const contentType = response.headers.get('content-type') ?? '';
      const payload: unknown = jsonTypes.some((type) => contentType.includes(type)) ? await response.json() : await response.text();
      if (!response.ok) { const problem = isProblemDetails(payload) ? payload : undefined; throw new ApiError({ code: statusCodeToErrorCode(response.status), message: problem?.title ?? 'La API rechazó la solicitud.', status: response.status, traceId: problem?.traceId, fieldErrors: problem?.errors }); }
      return payload as T;
    } catch (error) {
      if (error instanceof ApiError) throw error;
      if (requestOptions.signal?.aborted) throw new ApiError({ code: 'CANCELLED', message: 'Solicitud cancelada.', cause: error });
      if (signalState.timeoutTriggered()) throw new ApiError({ code: 'TIMEOUT', message: 'Tiempo de espera agotado.', cause: error });
      throw new ApiError({ code: 'NETWORK_ERROR', message: 'Error de red.', cause: error });
    } finally { signalState.cleanup(); }
  }
}

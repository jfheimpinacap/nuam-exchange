export type QueryValue = string | number | boolean | null | undefined;
export type RequestBody = object | FormData | undefined;
export interface HttpClientOptions { baseUrl: string; timeoutMs: number; getAccessToken?: () => string | undefined | Promise<string | undefined>; }
export interface RequestOptions { signal?: AbortSignal; headers?: Record<string, string>; query?: { [key: string]: QueryValue }; body?: RequestBody; }

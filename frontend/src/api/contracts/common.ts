export interface ApiResponse<T> { data: T; message?: string; traceId?: string; }
export interface PaginatedResponse<T> { items: T[]; page: number; pageSize: number; totalItems: number; totalPages: number; }
export interface PaginationRequest { page: number; pageSize: number; }
export type SortDirection = 'asc' | 'desc';
export type ValidationErrors = Record<string, string[]>;
export interface ProblemDetailsDto { type?: string; title: string; status: number; detail?: string; instance?: string; traceId?: string; errors?: ValidationErrors; }
export type ApiErrorCode = 'NETWORK_ERROR' | 'TIMEOUT' | 'VALIDATION_ERROR' | 'UNAUTHORIZED' | 'FORBIDDEN' | 'NOT_FOUND' | 'CONFLICT' | 'SERVER_ERROR' | 'INVALID_RESPONSE' | 'CONFIGURATION_ERROR' | 'CANCELLED';

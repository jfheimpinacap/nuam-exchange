import type { ApiErrorCode, ValidationErrors } from '../contracts/common';

export class ApiError extends Error {
  readonly code: ApiErrorCode; readonly status?: number; readonly traceId?: string; readonly fieldErrors?: ValidationErrors; override readonly cause?: unknown;
  constructor(params: { code: ApiErrorCode; message: string; status?: number; traceId?: string; fieldErrors?: ValidationErrors; cause?: unknown }) {
    super(params.message); this.name = 'ApiError'; this.code = params.code; this.status = params.status; this.traceId = params.traceId; this.fieldErrors = params.fieldErrors; this.cause = params.cause;
  }
}
export function isApiError(error: unknown): error is ApiError { return error instanceof ApiError; }
export function getUserFriendlyApiMessage(error: unknown): string {
  if (!isApiError(error)) return 'No fue posible completar la solicitud.';
  if (error.code === 'CANCELLED') return 'La solicitud fue cancelada.';
  if (error.code === 'TIMEOUT') return 'La solicitud tardó más de lo esperado. Intente nuevamente.';
  if (error.code === 'NETWORK_ERROR') return 'No fue posible conectar con el servidor.';
  if (error.code === 'VALIDATION_ERROR') return 'Revise los datos ingresados e intente nuevamente.';
  if (error.status === 401 || error.code === 'UNAUTHORIZED') return 'Su sesión no es válida o ha expirado.';
  if (error.status === 403 || error.code === 'FORBIDDEN') return 'No tiene permisos para realizar esta acción.';
  if (error.status === 404 || error.code === 'NOT_FOUND') return 'El recurso solicitado no fue encontrado.';
  if (error.status === 409 || error.code === 'CONFLICT') return 'Los datos fueron modificados por otro usuario o existe un conflicto.';
  if ((error.status && error.status >= 500) || error.code === 'SERVER_ERROR') return 'El servidor no pudo completar la solicitud.';
  return error.message || 'No fue posible completar la solicitud.';
}

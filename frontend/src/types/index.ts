export type UserRole = 'Administrador' | 'Analista Tributario' | 'Supervisor';
export type ClassificationStatus = 'Borrador' | 'Pendiente' | 'Aprobada' | 'Rechazada';
export type DataSourceMode = 'mock' | 'api';
export interface SessionUser { id: string; name: string; email: string; role: UserRole; }
export interface TaxClassification { id: string; sequence: string; market: string; source: string; instrument: string; description: string; fiscalYear: number; paymentDate: string; amount: number; factor: number; status: ClassificationStatus; updatedAt: string; }
export interface PagedResult<T> { items: T[]; total: number; page: number; pageSize: number; }
export interface UploadReview { id: string; type: 'X_FACTOR' | 'X_MONTO'; fileName: string; status: 'Validada' | 'Con errores' | 'Procesada'; validRows: number; errorRows: number; createdAt: string; }
export interface AuditEvent { id: string; date: string; user: string; role: UserRole; module: string; action: string; result: string; severity: 'Info' | 'Advertencia' | 'Crítica'; detail: string; }
export interface BackupRecord { id: string; name: string; date: string; status: 'Completado' | 'En progreso' | 'Programado' | 'Cancelado'; size: string; createdBy: string; }

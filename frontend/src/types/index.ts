export type Role = 'Administrador' | 'Analista Tributario' | 'Supervisor';
export type ClassificationStatus = 'Borrador' | 'Pendiente' | 'Aprobada' | 'Rechazada';
export interface Classification { id:string; market:string; instrument:string; description:string; fiscalYear:number; paymentDate:string; amount:number; status:ClassificationStatus; }
export interface AuditEvent { id:string; date:string; user:string; module:string; action:string; result:string; severity:'Info'|'Advertencia'|'Crítica'; }

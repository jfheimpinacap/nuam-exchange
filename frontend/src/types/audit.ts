import type { UserRole } from './session';
export type AuditModule = 'Autenticación'|'Calificaciones'|'Cargas'|'Reportes'|'Usuarios'|'Roles y Permisos'|'Auditoría'|'Respaldos';
export type AuditAction = 'Inicio de sesión'|'Cierre de sesión'|'Consulta'|'Creación'|'Edición'|'Eliminación'|'Carga'|'Exportación'|'Cambio de estado'|'Cambio de permisos'|'Respaldo'|'Restauración';
export type AuditResult = 'Exitoso'|'Advertencia'|'Fallido';
export type AuditSeverity = 'Informativa'|'Media'|'Alta';
export interface AuditEvent { id:string; fechaHora:string; usuarioId:string; usuarioNombre:string; rol:UserRole; modulo:AuditModule; accion:AuditAction; entidad:string; entidadId:string; descripcion:string; resultado:AuditResult; severidad:AuditSeverity; origen:'Interfaz web'|'Proceso programado'|'Módulo administrativo'; detalles: Record<string,string>; }
export interface AuditFilters { texto:string; desde:string; hasta:string; usuario:string; rol:'Todos'|UserRole; modulo:'Todos'|AuditModule; accion:'Todos'|AuditAction; resultado:'Todos'|AuditResult; severidad:'Todos'|AuditSeverity; }
export type AuditSortKey = 'fechaHora'|'usuarioNombre'|'modulo'|'accion'|'resultado'|'severidad';
export interface AuditSortState { key: AuditSortKey; direction:'asc'|'desc'; }

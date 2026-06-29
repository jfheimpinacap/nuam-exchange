import type { AuditEvent } from '../types/audit';
const base: Omit<AuditEvent,'id'|'fechaHora'|'usuarioId'|'usuarioNombre'|'rol'>[] = [
{modulo:'Autenticación',accion:'Inicio de sesión',entidad:'Sesión',entidadId:'SES-001',descripcion:'Inicio de sesión exitoso en entorno demo.',resultado:'Exitoso',severidad:'Informativa',origen:'Interfaz web',detalles:{navegador:'Cliente web simulado'}},
{modulo:'Autenticación',accion:'Inicio de sesión',entidad:'Sesión',entidadId:'SES-002',descripcion:'Intento de inicio fallido simulado.',resultado:'Fallido',severidad:'Alta',origen:'Interfaz web',detalles:{motivo:'Credenciales ficticias no válidas'}},
{modulo:'Calificaciones',accion:'Creación',entidad:'Calificación',entidadId:'CAL-2024-010',descripcion:'Creación de calificación tributaria simulada.',resultado:'Exitoso',severidad:'Informativa',origen:'Interfaz web',detalles:{mercado:'Renta Fija'}},
{modulo:'Calificaciones',accion:'Edición',entidad:'Calificación',entidadId:'CAL-2025-022',descripcion:'Edición controlada de campos tributarios mockeados.',resultado:'Advertencia',severidad:'Media',origen:'Interfaz web',detalles:{campo:'Factor tributario'}},
{modulo:'Cargas',accion:'Carga',entidad:'Archivo X Factor',entidadId:'XF-2026-004',descripcion:'Carga X Factor procesada con filas observadas.',resultado:'Advertencia',severidad:'Media',origen:'Interfaz web',detalles:{filas:'128'}},
{modulo:'Cargas',accion:'Carga',entidad:'Archivo X Monto',entidadId:'XM-2026-002',descripcion:'Carga X Monto finalizada correctamente.',resultado:'Exitoso',severidad:'Informativa',origen:'Interfaz web',detalles:{filas:'94'}},
{modulo:'Reportes',accion:'Exportación',entidad:'Reporte',entidadId:'REP-CAL',descripcion:'Exportación CSV de reporte simulado.',resultado:'Exitoso',severidad:'Informativa',origen:'Interfaz web',detalles:{formato:'CSV'}},
{modulo:'Usuarios',accion:'Creación',entidad:'Usuario',entidadId:'usr-demo-021',descripcion:'Creación de usuario administrativo ficticio.',resultado:'Exitoso',severidad:'Media',origen:'Módulo administrativo',detalles:{rolAsignado:'Analista Tributario'}},
{modulo:'Usuarios',accion:'Cambio de estado',entidad:'Usuario',entidadId:'usr-demo-012',descripcion:'Bloqueo simulado de usuario.',resultado:'Advertencia',severidad:'Alta',origen:'Módulo administrativo',detalles:{estado:'Bloqueado'}},
{modulo:'Roles y Permisos',accion:'Cambio de permisos',entidad:'Matriz de permisos',entidadId:'ROL-SUP',descripcion:'Cambio visual de permisos para rol Supervisor.',resultado:'Exitoso',severidad:'Media',origen:'Módulo administrativo',detalles:{persistencia:'No aplicada'}},
{modulo:'Respaldos',accion:'Respaldo',entidad:'Respaldo',entidadId:'BKP-2026-006',descripcion:'Creación de respaldo simulado.',resultado:'Exitoso',severidad:'Informativa',origen:'Proceso programado',detalles:{tipo:'Completo'}},
{modulo:'Respaldos',accion:'Restauración',entidad:'Respaldo',entidadId:'BKP-2025-011',descripcion:'Restauración simulada finalizada sin modificar datos.',resultado:'Exitoso',severidad:'Alta',origen:'Módulo administrativo',detalles:{confirmacion:'RESTAURAR'}},
{modulo:'Auditoría',accion:'Consulta',entidad:'Bitácora',entidadId:'AUD-FLT',descripcion:'Consulta administrativa de eventos filtrados.',resultado:'Exitoso',severidad:'Informativa',origen:'Interfaz web',detalles:{filtro:'Rango anual'}},
];
const users = [
  ['adm-001','Usuario Administrador','Administrador'], ['ana-reportes-008','Analista Reportes','Analista Tributario'], ['sup-demo-003','Supervisor Demo','Supervisor'],
] as const;
const dates = ['2024-02-12T09:10:00','2024-05-18T14:25:00','2024-09-02T11:40:00','2025-01-20T08:05:00','2025-04-15T16:30:00','2025-08-28T10:55:00','2026-01-09T13:15:00','2026-03-22T17:45:00','2026-06-18T12:05:00'];
export const auditEvents: AuditEvent[] = Array.from({length:39},(_,i)=>{ const b=base[i%base.length]; const u=users[i%users.length]; return { id:`AUD-${String(i+1).padStart(3,'0')}`, fechaHora:dates[i%dates.length], usuarioId:u[0], usuarioNombre:u[1], rol:u[2], ...b, entidadId:`${b.entidadId}-${String(i+1).padStart(2,'0')}`}; });

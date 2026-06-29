import { dateStamp, downloadCsv } from '../../utils/csvExport';
import type { AuditEvent } from '../../types/audit';
import { formatDateTime } from './auditUtils';
export function exportAudit(rows:AuditEvent[]){ downloadCsv(`auditoria-${dateStamp()}.csv`, rows, [
 {header:'ID',value:r=>r.id},{header:'Fecha y hora',value:r=>formatDateTime(r.fechaHora)},{header:'Usuario',value:r=>r.usuarioNombre},{header:'Rol',value:r=>r.rol},{header:'Módulo',value:r=>r.modulo},{header:'Acción',value:r=>r.accion},{header:'Entidad',value:r=>r.entidad},{header:'Entidad ID',value:r=>r.entidadId},{header:'Resultado',value:r=>r.resultado},{header:'Severidad',value:r=>r.severidad},{header:'Origen',value:r=>r.origen},{header:'Descripción',value:r=>r.descripcion},]); }

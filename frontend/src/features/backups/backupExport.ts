import { downloadCsv } from '../../utils/csvExport';
import type { BackupRecord } from '../../types/backup';
import { formatBackupDate, formatBytes } from './backupUtils';
export function exportBackupManifest(b:BackupRecord){ downloadCsv(`manifiesto-respaldo-${b.id}.csv`, [b], [
{header:'ID',value:r=>r.id},{header:'Tipo',value:r=>r.tipo},{header:'Alcance',value:r=>r.alcance},{header:'Fecha de inicio',value:r=>formatBackupDate(r.fechaInicio)},{header:'Fecha de término',value:r=>formatBackupDate(r.fechaTermino)},{header:'Estado',value:r=>r.estado},{header:'Tamaño',value:r=>formatBytes(r.tamañoBytes)},{header:'Registros incluidos',value:r=>r.registrosIncluidos},{header:'Ejecutado por',value:r=>r.ejecutadoPor},{header:'Descripción',value:r=>r.descripcion},{header:'Observaciones',value:r=>r.observaciones}]); }

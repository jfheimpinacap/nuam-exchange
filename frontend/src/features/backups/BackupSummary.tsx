import type { BackupRecord } from '../../types/backup';
import { formatBytes, summarizeBackups } from './backupUtils';
export function BackupSummary({ backups }: { backups: BackupRecord[] }){ const s=summarizeBackups(backups); const rows=[['Total',s.total],['Completados',s.completados],['En proceso',s.proceso],['Fallidos',s.fallidos],['Programados',s.programados],['Espacio total simulado',formatBytes(s.espacio)]]; return <dl className="metrics-grid">{rows.map(([l,v])=><div className="metric-card" key={l}><dt><span>{l}</span></dt><dd><strong>{v}</strong></dd></div>)}</dl>; }

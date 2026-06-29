import type { AuditEvent } from '../../types/audit';
import { auditSummary } from './auditUtils';
export function AuditSummary({ events }: { events: AuditEvent[] }) { const s=auditSummary(events); const cards=[['Total de eventos',s.total],['Exitosos',s.exitosos],['Advertencias',s.advertencias],['Fallidos',s.fallidos],['Severidad alta',s.alta],['Usuarios involucrados',s.usuarios]]; return <dl className="metrics-grid">{cards.map(([label,value])=><div className="metric-card" key={label}><dt><span>{label}</span></dt><dd><strong>{value}</strong></dd></div>)}</dl>; }

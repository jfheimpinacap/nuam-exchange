import { PageHeader } from '../components/PageHeader';
import { StatusBadge } from '../components/StatusBadge';
import { auditEvents } from '../mocks/classifications';
export function AuditPage() { return <section className="page"><PageHeader title="Auditoría" description="Trazabilidad ficticia con filtros y detalle visual." /><div className="actions"><input placeholder="Buscar usuario, módulo o acción" /><button type="button">Exportar CSV visual</button></div><table><tbody>{auditEvents.map(event=><tr key={event.id}><td>{event.date}</td><td>{event.user}</td><td>{event.module}</td><td>{event.action}</td><td><StatusBadge value={event.severity} /></td><td>{event.detail}</td></tr>)}</tbody></table></section>; }

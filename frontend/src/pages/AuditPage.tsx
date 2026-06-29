import { mockAuditEvents } from '../mocks/classifications';
export function AuditPage() { return <section className="page"><h1>Auditoría</h1><div className="cards">{mockAuditEvents.map((event) => <article className="card" key={event.id}><strong>{event.action}</strong><span>{event.date} · {event.user}</span><p>{event.module} — {event.result} ({event.severity})</p></article>)}</div></section>; }

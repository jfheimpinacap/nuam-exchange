import { Link } from 'react-router-dom';
import { StatusBadge } from '../../components/StatusBadge';
import type { TaxClassification } from '../../types';
import { moneyFormatter } from '../../utils/formatters';
export function ClassificationsTable({ rows }: { rows: TaxClassification[] }) { return <table><thead><tr><th>Secuencia</th><th>Mercado</th><th>Instrumento</th><th>Ejercicio</th><th>Pago</th><th>Monto</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>{rows.map(row=><tr key={row.id}><td>{row.sequence}</td><td>{row.market}</td><td><strong>{row.instrument}</strong><span>{row.description}</span></td><td>{row.fiscalYear}</td><td>{row.paymentDate}</td><td>{moneyFormatter.format(row.amount)}</td><td><StatusBadge value={row.status} /></td><td><Link to={`/calificaciones/${row.id}/editar`}>Editar</Link> · <Link to={`/calificaciones/${row.id}/copiar`}>Copiar</Link></td></tr>)}</tbody></table>; }

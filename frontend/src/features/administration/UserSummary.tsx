import type { AdministrationUser } from '../../types/administration';
import { summarizeUsers } from './administrationUtils';

export function UserSummary({ users }: { users: AdministrationUser[] }) {
  const summary = summarizeUsers(users);
  const items = [
    ['Total', summary.total], ['Activos', summary.activos], ['Inactivos', summary.inactivos], ['Bloqueados', summary.bloqueados],
    ['Administradores', summary.administradores], ['Analistas', summary.analistas], ['Supervisores', summary.supervisores],
  ];
  return <dl className="admin-summary">{items.map(([label, value]) => <div key={label}><dt>{label}</dt><dd>{value}</dd></div>)}</dl>;
}

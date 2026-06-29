import { DemoModeBadge } from '../components/DemoModeBadge';
import { roles, useSession } from '../app/session';
import { useNavigation, type RoutePath } from '../app/navigation';

const links: ReadonlyArray<[string, RoutePath]> = [
  ['Inicio', '/inicio'], ['Calificaciones', '/calificaciones'], ['Cargas', '/cargas'], ['Reportes', '/reportes'],
  ['Usuarios', '/administracion/usuarios'], ['Roles y permisos', '/administracion/roles-permisos'], ['Auditoría', '/auditoria'], ['Respaldos', '/respaldos']
];

export function AdminLayout({ children }: { children: React.ReactNode }) {
  const { role, setRole, userName } = useSession();
  const { path, navigate } = useNavigation();
  return <div className="shell">
    <aside className="sidebar"><div className="brand">Nuam Exchange</div><nav>{links.map(([label, to]) => <button className={path === to ? 'active nav-button' : 'nav-button'} key={to} type="button" onClick={() => navigate(to)}>{label}</button>)}</nav></aside>
    <main className="content"><header className="topbar"><div><strong>{userName}</strong><span>{role}</span></div><label>Perfil demo<select value={role} onChange={(event) => setRole(event.target.value as typeof role)}>{roles.map((item) => <option key={item}>{item}</option>)}</select></label><DemoModeBadge /></header>{children}</main>
  </div>;
}

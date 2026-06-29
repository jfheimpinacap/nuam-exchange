import { useSession } from '../app/session/useSession';
import { DataSourceIndicator } from './DataSourceIndicator';
export function Header() { const { user, logout } = useSession(); return <header className="topbar"><div><strong>{user?.name}</strong><span>{user?.role}</span></div><DataSourceIndicator /><button type="button" onClick={logout}>Cerrar sesión</button></header>; }

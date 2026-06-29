import { NavLink } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import { getNavigationForRole } from '../routes/navigation';
const groupLabels: Record<string, string> = { '/inicio': 'Operación', '/administracion/usuarios': 'Administración' };

interface SidebarProps {
  isCollapsed: boolean;
  isMobileOpen: boolean;
  onCloseMobile: () => void;
}

export function Sidebar({ isCollapsed, isMobileOpen, onCloseMobile }: SidebarProps) {
  const { user } = useSession();
  const authorizedItems = user ? getNavigationForRole(user.rol) : [];

  return (
    <aside className={`sidebar ${isCollapsed ? 'is-collapsed' : ''} ${isMobileOpen ? 'is-open' : ''}`}>
      <div className="sidebar-brand">
        <strong>Nuam Exchange</strong>
        <span>Sistema Tributario</span>
      </div>
      <nav aria-label="Menú principal">
        {authorizedItems.map((item) => (
          <div key={item.path}>
            {groupLabels[item.path] ? <p className="sidebar-group">{groupLabels[item.path]}</p> : null}
          <NavLink
            to={item.path}
            className={({ isActive }) => `sidebar-link ${isActive ? 'is-active' : ''}`}
            onClick={onCloseMobile}
          >
            <span className="sidebar-marker" aria-hidden="true" />
            <span>{item.label}</span>
          </NavLink>
          </div>
        ))}
      </nav>
    </aside>
  );
}

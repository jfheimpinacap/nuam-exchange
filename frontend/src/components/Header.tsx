import { useNavigate } from 'react-router-dom';
import { useSession } from '../app/session/useSession';

interface HeaderProps {
  isCollapsed: boolean;
  onToggleSidebar: () => void;
  onToggleMobile: () => void;
}

export function Header({ isCollapsed, onToggleSidebar, onToggleMobile }: HeaderProps) {
  const navigate = useNavigate();
  const { logout, user } = useSession();

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <header className="app-header">
      <div className="header-actions">
        <button className="icon-button desktop-only" type="button" onClick={onToggleSidebar} aria-pressed={isCollapsed}>
          {isCollapsed ? 'Expandir menú' : 'Colapsar menú'}
        </button>
        <button className="icon-button mobile-only" type="button" onClick={onToggleMobile}>
          Abrir menú
        </button>
        <div>
          <strong>Nuam Exchange</strong>
          <span>Sistema de Gestión Tributaria</span><span className="demo-chip">Modo demostración</span>
        </div>
      </div>
      <div className="user-area">
        <span className="user-summary">
          <strong>{user?.nombre}</strong>
          <small>{user?.rol}</small>
        </span>
        <button type="button" className="secondary-button" onClick={handleLogout}>Cerrar sesión</button>
      </div>
    </header>
  );
}

import { useLocation, useNavigate } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import { getSectionMetadata } from '../routes/navigation';

interface HeaderProps {
  onToggleMobile: () => void;
}

export function Header({ onToggleMobile }: HeaderProps) {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { logout, user } = useSession();
  const section = getSectionMetadata(pathname);

  const handleLogout = () => {
    logout();
    navigate('/login', { replace: true });
  };

  return (
    <header className="app-header">
      <div className="header-actions">
        <button className="icon-button mobile-only" type="button" onClick={onToggleMobile}>
          Abrir menú
        </button>
        <div className="section-heading">
          <strong>{section.title}</strong>
          <span>{section.description}</span>
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

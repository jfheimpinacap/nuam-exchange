import { Navigate, Outlet } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import { isPathAllowedForRole } from './navigation';

interface ProtectedRouteProps {
  routePath?: string;
}

export function ProtectedRoute({ routePath }: ProtectedRouteProps) {
  const { isAuthenticated, user } = useSession();

  if (!isAuthenticated || !user) {
    return <Navigate to="/login" replace />;
  }

  if (routePath && !isPathAllowedForRole(user.rol, routePath)) {
    return <Navigate to="/sin-acceso" replace />;
  }

  return <Outlet />;
}

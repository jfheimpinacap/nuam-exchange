import { Navigate, useLocation } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import type { UserRole } from '../types';
export function ProtectedRoute({ roles, children }: { roles?: UserRole[]; children: React.ReactNode }) { const { isAuthenticated, hasRole } = useSession(); const location = useLocation(); if(!isAuthenticated) return <Navigate to="/login" replace state={{ from: location }} />; if(roles && !hasRole(roles)) return <Navigate to="/acceso-denegado" replace />; return <>{children}</>; }

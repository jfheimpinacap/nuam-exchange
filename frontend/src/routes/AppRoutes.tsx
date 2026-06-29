import { Navigate, useLocation } from 'react-router-dom';
import { AppLayout } from '../layouts/AppLayout';
import { AccessDeniedPage } from '../pages/AccessDeniedPage';
import { AuditPage } from '../pages/AuditPage';
import { BackupsPage } from '../pages/BackupsPage';
import { ClassificationCopyPage } from '../pages/ClassificationCopyPage';
import { ClassificationCreatePage } from '../pages/ClassificationCreatePage';
import { ClassificationEditPage } from '../pages/ClassificationEditPage';
import { ClassificationsPage } from '../pages/ClassificationsPage';
import { InicioPage } from '../pages/InicioPage';
import { LoginPage } from '../pages/LoginPage';
import { ReportsPage } from '../pages/ReportsPage';
import { RolesPermissionsPage } from '../pages/RolesPermissionsPage';
import { UploadTemplatesPage } from '../pages/UploadTemplatesPage';
import { UsersAdministrationPage } from '../pages/UsersAdministrationPage';
import { XAmountUploadPage } from '../pages/XAmountUploadPage';
import { XFactorUploadPage } from '../pages/XFactorUploadPage';
import { ProtectedRoute } from './ProtectedRoute';
import { useSession } from '../app/session/useSession';
function pageFor(path: string) { if(path==='/inicio') return <InicioPage />; if(path==='/calificaciones') return <ClassificationsPage />; if(path==='/calificaciones/nueva') return <ClassificationCreatePage />; if(/^\/calificaciones\/[^/]+\/editar$/.test(path)) return <ClassificationEditPage />; if(/^\/calificaciones\/[^/]+\/copiar$/.test(path)) return <ClassificationCopyPage />; if(path==='/cargas/x-factor') return <XFactorUploadPage />; if(path==='/cargas/x-monto') return <XAmountUploadPage />; if(path==='/plantillas-carga') return <UploadTemplatesPage />; if(path==='/reportes') return <ReportsPage />; if(path==='/administracion/usuarios') return <UsersAdministrationPage />; if(path==='/administracion/roles-permisos') return <RolesPermissionsPage />; if(path==='/auditoria') return <AuditPage />; if(path==='/respaldos') return <BackupsPage />; return null; }
function adminOnly(path: string) { return ['/administracion/usuarios','/administracion/roles-permisos','/auditoria','/respaldos'].includes(path); }
export function AppRoutes() { const { isAuthenticated } = useSession(); const { pathname } = useLocation(); if(pathname==='/login') return <LoginPage />; if(pathname==='/acceso-denegado') return <AccessDeniedPage />; const page=pageFor(pathname); if(!page) return <Navigate to={isAuthenticated ? '/inicio' : '/login'} replace />; return <ProtectedRoute roles={adminOnly(pathname) ? ['Administrador'] : undefined}><AppLayout>{page}</AppLayout></ProtectedRoute>; }

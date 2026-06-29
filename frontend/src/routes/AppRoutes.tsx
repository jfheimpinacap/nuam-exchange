import { Navigate, Route, Routes } from 'react-router-dom';
import { AppLayout } from '../layouts/AppLayout';
import { AccessDeniedPage } from '../pages/AccessDeniedPage';
import { InicioPage } from '../pages/InicioPage';
import { LoginPage } from '../pages/LoginPage';
import { UsersAdministrationPage } from '../pages/UsersAdministrationPage';
import { RolesPermissionsPage } from '../pages/RolesPermissionsPage';
import { ReportsPage } from '../pages/ReportsPage';
import { ClassificationsPage } from '../pages/ClassificationsPage';
import { ClassificationCreatePage } from '../pages/ClassificationCreatePage';
import { ClassificationEditPage } from '../pages/ClassificationEditPage';
import { ClassificationCopyPage } from '../pages/ClassificationCopyPage';
import { XFactorUploadPage } from '../pages/XFactorUploadPage';
import { XAmountUploadPage } from '../pages/XAmountUploadPage';
import { UploadTemplatesPage } from '../pages/UploadTemplatesPage';
import { AuditPage } from '../pages/AuditPage';
import { BackupsPage } from '../pages/BackupsPage';
import { ProtectedRoute } from './ProtectedRoute';
import { useSession } from '../app/session/useSession';

export function AppRoutes() {
  const { isAuthenticated } = useSession();
  const fallbackPath = isAuthenticated ? '/inicio' : '/login';

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/sin-acceso" element={<AccessDeniedPage />} />
          <Route element={<ProtectedRoute routePath="/inicio" />}>
            <Route path="/inicio" element={<InicioPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/calificaciones" />}>
            <Route path="/calificaciones" element={<ClassificationsPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/calificaciones/nueva" />}>
            <Route path="/calificaciones/nueva" element={<ClassificationCreatePage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/calificaciones/:id/editar" />}>
            <Route path="/calificaciones/:id/editar" element={<ClassificationEditPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/calificaciones/:id/copiar" />}>
            <Route path="/calificaciones/:id/copiar" element={<ClassificationCopyPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/cargas/x-factor" />}>
            <Route path="/cargas/x-factor" element={<XFactorUploadPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/cargas/x-monto" />}>
            <Route path="/cargas/x-monto" element={<XAmountUploadPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/plantillas-carga" />}>
            <Route path="/plantillas-carga" element={<UploadTemplatesPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/reportes" />}>
            <Route path="/reportes" element={<ReportsPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/administracion/usuarios" />}>
            <Route path="/administracion/usuarios" element={<UsersAdministrationPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/administracion/roles-permisos" />}>
            <Route path="/administracion/roles-permisos" element={<RolesPermissionsPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/auditoria" />}>
            <Route path="/auditoria" element={<AuditPage />} />
          </Route>
          <Route element={<ProtectedRoute routePath="/respaldos" />}>
            <Route path="/respaldos" element={<BackupsPage />} />
          </Route>
        </Route>
      </Route>
      <Route path="*" element={<Navigate to={fallbackPath} replace />} />
    </Routes>
  );
}

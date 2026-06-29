import { useNavigation } from '../app/navigation';
import { AdminUsersPage } from '../pages/AdminUsersPage';
import { AuditPage } from '../pages/AuditPage';
import { BackupsPage } from '../pages/BackupsPage';
import { ClassificationsPage } from '../pages/ClassificationsPage';
import { DashboardPage } from '../pages/DashboardPage';
import { ReportsPage } from '../pages/ReportsPage';
import { RolesPage } from '../pages/RolesPage';
import { UploadsPage } from '../pages/UploadsPage';

export function AppRoutes() {
  const { path } = useNavigation();
  switch (path) {
    case '/inicio': return <DashboardPage />;
    case '/calificaciones': return <ClassificationsPage />;
    case '/cargas': return <UploadsPage />;
    case '/reportes': return <ReportsPage />;
    case '/administracion/usuarios': return <AdminUsersPage />;
    case '/administracion/roles-permisos': return <RolesPage />;
    case '/auditoria': return <AuditPage />;
    case '/respaldos': return <BackupsPage />;
  }
}

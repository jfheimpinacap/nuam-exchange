import type { AdministrationUser } from '../../types/administration';
import type { RolePermissionMap } from '../../types/permission';
import { countUsersByRole, countVisibleModules, roleDefinitions } from './permissionCatalog';
export function RoleSummary({ users, permissions }: { users: AdministrationUser[]; permissions: RolePermissionMap }) {
  return <dl className="role-summary">{roleDefinitions.map((role) => <div key={role.role}><dt>{role.role}</dt><dd>{role.description}</dd><dd>{countUsersByRole(role.role, users)} usuarios · {countVisibleModules(permissions[role.role])} módulos · {permissions[role.role].length} permisos</dd><dd><strong>{role.status}</strong></dd></div>)}</dl>;
}

import type { PermissionCode } from '../../types/permission';
import type { UserRole } from '../../types/session';
import { permissionActions, permissionCode, permissionModules } from './permissionCatalog';

interface Props { role: UserRole; permissions: PermissionCode[]; savedPermissions: PermissionCode[]; onChange: (permissions: PermissionCode[]) => void; }
export function PermissionsMatrix({ role, permissions, savedPermissions, onChange }: Props) {
  const isAdmin = role === 'Administrador';
  const toggle = (code: PermissionCode) => {
    if (isAdmin) return;
    const exists = permissions.includes(code);
    const [moduleCode, action] = code.split(':');
    let next = exists ? permissions.filter((item) => item !== code) : [...permissions, code];
    if (action === 'Ver' && exists) next = next.filter((item) => !item.startsWith(`${moduleCode}:`));
    if (action !== 'Ver' && !permissions.includes(`${moduleCode}:Ver` as PermissionCode)) next = [...next, `${moduleCode}:Ver` as PermissionCode];
    onChange(next);
  };
  const changes = permissions.filter((item) => !savedPermissions.includes(item)).length + savedPermissions.filter((item) => !permissions.includes(item)).length;
  return <section className="permissions-panel"><p>{isAdmin ? 'Administrador es el rol de control total; sus permisos están bloqueados.' : 'Los cambios editan una copia local y no alteran ProtectedRoute.'}</p><p aria-live="polite">Cambios pendientes: {changes}</p><div className="table-scroll"><table className="data-table permission-table"><caption>Matriz de permisos por rol</caption><thead><tr><th>Módulo</th>{permissionActions.map((action) => <th key={action}>{action}</th>)}</tr></thead><tbody>{permissionModules.map((module) => { const hasView = permissions.includes(permissionCode(module.code, 'Ver')); return <tr key={module.code}><th>{module.label}</th>{permissionActions.map((action) => { const available = module.actions.includes(action); const code = permissionCode(module.code, action); const disabled = isAdmin || !available || (action !== 'Ver' && !hasView); return <td key={action}>{available ? <input type="checkbox" aria-label={`${module.label}: ${action}`} checked={permissions.includes(code)} disabled={disabled} onChange={() => toggle(code)} /> : <span aria-label="No aplica">—</span>}</td>; })}</tr>; })}</tbody></table></div></section>;
}

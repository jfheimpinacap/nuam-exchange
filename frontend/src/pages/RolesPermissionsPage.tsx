import { useMemo, useState } from 'react';
import { Button } from '../components/Button';
import { InlineMessage } from '../components/InlineMessage';
import { PermissionsMatrix } from '../features/administration/PermissionsMatrix';
import { RoleSelector } from '../features/administration/RoleSelector';
import { RoleSummary } from '../features/administration/RoleSummary';
import { defaultRolePermissions, enforcePermissionDependencies, roleDefinitions } from '../features/administration/permissionCatalog';
import { administrationUsers } from '../mocks/administrationUsers';
import type { PermissionCode, RolePermissionMap } from '../types/permission';
import type { UserRole } from '../types/session';

export function RolesPermissionsPage() {
  const [selectedRole, setSelectedRole] = useState<UserRole>('Administrador');
  const [saved, setSaved] = useState<RolePermissionMap>(defaultRolePermissions);
  const [draft, setDraft] = useState<RolePermissionMap>(defaultRolePermissions);
  const [message, setMessage] = useState('');
  const role = useMemo(() => roleDefinitions.find((item) => item.role === selectedRole), [selectedRole]);
  const updateDraft = (permissions: PermissionCode[]) => setDraft({ ...draft, [selectedRole]: enforcePermissionDependencies(permissions) });
  const reset = () => { setDraft({ ...draft, [selectedRole]: defaultRolePermissions[selectedRole] }); setMessage('Valores predeterminados restablecidos para el rol seleccionado.'); };
  const discard = () => { setDraft({ ...draft, [selectedRole]: saved[selectedRole] }); setMessage('Cambios descartados. Se mantiene la última versión guardada en memoria.'); };
  const save = () => { const next = { ...saved, [selectedRole]: enforcePermissionDependencies(draft[selectedRole]) }; setSaved(next); setDraft(next); setMessage('Permisos actualizados en la demostración. No se modificó la seguridad real de la aplicación.'); };
  return <section className="admin-page">{message && <InlineMessage tone="success" message={message} />}<RoleSummary users={administrationUsers} permissions={saved} /><><section className="permissions-panel"><RoleSelector selectedRole={selectedRole} onSelect={setSelectedRole} /><h2>{selectedRole}</h2><p>{role?.description}</p><p>{administrationUsers.filter((user) => user.rol === selectedRole).length} usuarios asociados. Los roles son fijos y no pueden crearse, eliminarse ni renombrarse.</p></section><PermissionsMatrix role={selectedRole} permissions={draft[selectedRole]} savedPermissions={saved[selectedRole]} onChange={updateDraft} /><div className="actions-bar"><Button variant="primary" disabled={selectedRole === 'Administrador'} onClick={save}>Guardar cambios simulados</Button><Button disabled={selectedRole === 'Administrador'} onClick={reset}>Restablecer valores</Button><Button disabled={selectedRole === 'Administrador'} onClick={discard}>Descartar cambios</Button></div></></section>;
}

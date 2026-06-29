import { useMemo, useState } from 'react';
import { Button } from '../components/Button';
import { InlineMessage } from '../components/InlineMessage';
import { PageHeader } from '../components/PageHeader';
import { ErrorState, LoadingState } from '../components/ViewStates';
import { PermissionsMatrix } from '../features/administration/PermissionsMatrix';
import { RoleSelector } from '../features/administration/RoleSelector';
import { RoleSummary } from '../features/administration/RoleSummary';
import { defaultRolePermissions, enforcePermissionDependencies, roleDefinitions } from '../features/administration/permissionCatalog';
import { administrationUsers } from '../mocks/administrationUsers';
import type { DemoViewState } from '../types/administration';
import type { PermissionCode, RolePermissionMap } from '../types/permission';
import type { UserRole } from '../types/session';

export function RolesPermissionsPage() {
  const [selectedRole, setSelectedRole] = useState<UserRole>('Administrador');
  const [saved, setSaved] = useState<RolePermissionMap>(defaultRolePermissions);
  const [draft, setDraft] = useState<RolePermissionMap>(defaultRolePermissions);
  const [viewState, setViewState] = useState<DemoViewState>('normal');
  const [message, setMessage] = useState('');
  const role = useMemo(() => roleDefinitions.find((item) => item.role === selectedRole), [selectedRole]);
  const updateDraft = (permissions: PermissionCode[]) => setDraft({ ...draft, [selectedRole]: enforcePermissionDependencies(permissions) });
  const reset = () => { setDraft({ ...draft, [selectedRole]: defaultRolePermissions[selectedRole] }); setMessage('Valores predeterminados restablecidos para el rol seleccionado.'); };
  const discard = () => { setDraft({ ...draft, [selectedRole]: saved[selectedRole] }); setMessage('Cambios descartados. Se mantiene la última versión guardada en memoria.'); };
  const save = () => { const next = { ...saved, [selectedRole]: enforcePermissionDependencies(draft[selectedRole]) }; setSaved(next); setDraft(next); setMessage('Permisos actualizados en la demostración. No se modificó la seguridad real de la aplicación.'); };
  return <section className="admin-page"><PageHeader title="Roles y Permisos" description="Configuración visual y simulada de capacidades por rol." /><div className="demo-panel"><label htmlFor="roles-demo-state">Estado de demostración</label><select id="roles-demo-state" value={viewState} onChange={(e) => setViewState(e.target.value as DemoViewState)}><option value="normal">Normal</option><option value="loading">Cargando</option><option value="error">Error</option></select><span>La seguridad real sigue dependiendo de ProtectedRoute y del backend futuro.</span></div>{message && <InlineMessage tone="success" message={message} />}<RoleSummary users={administrationUsers} permissions={saved} />{viewState === 'loading' && <LoadingState message="Cargando matriz de permisos..." />}{viewState === 'error' && <ErrorState title="Error de demostración" description="Estado simulado para validar la pantalla de error." />}{viewState === 'normal' && <><section className="permissions-panel"><RoleSelector selectedRole={selectedRole} onSelect={setSelectedRole} /><h2>{selectedRole}</h2><p>{role?.description}</p><p>{administrationUsers.filter((user) => user.rol === selectedRole).length} usuarios asociados. Los roles son fijos y no pueden crearse, eliminarse ni renombrarse.</p></section><PermissionsMatrix role={selectedRole} permissions={draft[selectedRole]} savedPermissions={saved[selectedRole]} onChange={updateDraft} /><div className="actions-bar"><Button variant="primary" disabled={selectedRole === 'Administrador'} onClick={save}>Guardar cambios simulados</Button><Button disabled={selectedRole === 'Administrador'} onClick={reset}>Restablecer valores</Button><Button disabled={selectedRole === 'Administrador'} onClick={discard}>Descartar cambios</Button></div></>}</section>;
}

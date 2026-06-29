import { Button } from '../../components/Button';
import type { UserRole } from '../../types/session';
import { roleDefinitions } from './permissionCatalog';
export function RoleSelector({ selectedRole, onSelect }: { selectedRole: UserRole; onSelect: (role: UserRole) => void }) {
  return <div className="tabs" role="tablist" aria-label="Roles fijos">{roleDefinitions.map((definition) => <Button key={definition.role} role="tab" aria-selected={selectedRole === definition.role} onClick={() => onSelect(definition.role)}>{definition.role}</Button>)}</div>;
}

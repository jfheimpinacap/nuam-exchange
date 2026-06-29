import { Button } from '../../components/Button';
import { FormField } from '../../components/FormField';
import type { UserFilters as UserFiltersType } from '../../types/administration';
import { userRoles, userStatuses } from './administrationUtils';

interface Props { draft: UserFiltersType; total: number; onChange: (filters: UserFiltersType) => void; onSearch: () => void; onClear: () => void; }
export function UserFilters({ draft, total, onChange, onSearch, onClear }: Props) {
  return <section className="filters-panel admin-filters" aria-label="Filtros de usuarios">
    <FormField id="user-search" label="Texto libre"><input id="user-search" value={draft.texto} onChange={(e) => onChange({ ...draft, texto: e.target.value })} placeholder="Nombre, correo o ID" /></FormField>
    <FormField id="user-role" label="Rol"><select id="user-role" value={draft.rol} onChange={(e) => onChange({ ...draft, rol: e.target.value as UserFiltersType['rol'] })}><option>Todos</option>{userRoles.map((role) => <option key={role}>{role}</option>)}</select></FormField>
    <FormField id="user-status" label="Estado"><select id="user-status" value={draft.estado} onChange={(e) => onChange({ ...draft, estado: e.target.value as UserFiltersType['estado'] })}><option>Todos</option>{userStatuses.map((status) => <option key={status}>{status}</option>)}</select></FormField>
    <div className="filter-actions"><Button variant="primary" onClick={onSearch}>Buscar</Button><Button onClick={onClear}>Limpiar</Button></div>
    <span aria-live="polite">{total} resultados</span>
  </section>;
}

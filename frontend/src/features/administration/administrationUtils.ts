import type { AdministrationUser, UserAccountStatus, UserFilters, UserSortState } from '../../types/administration';
import type { UserRole } from '../../types/session';

export const initialUserFilters: UserFilters = { texto: '', rol: 'Todos', estado: 'Todos' };
export const userRoles: UserRole[] = ['Administrador', 'Analista Tributario', 'Supervisor'];
export const userStatuses: UserAccountStatus[] = ['Activo', 'Inactivo', 'Bloqueado'];

export function filterUsers(users: AdministrationUser[], filters: UserFilters) {
  const text = filters.texto.trim().toLowerCase();
  return users.filter((user) => {
    const matchesText = !text || [user.nombre, user.email, user.id].some((value) => value.toLowerCase().includes(text));
    return matchesText && (filters.rol === 'Todos' || user.rol === filters.rol) && (filters.estado === 'Todos' || user.estado === filters.estado);
  });
}

export function sortUsers(users: AdministrationUser[], sort: UserSortState) {
  return [...users].sort((a, b) => {
    const left = a[sort.key] ?? '';
    const right = b[sort.key] ?? '';
    const result = String(left).localeCompare(String(right), 'es', { sensitivity: 'base' });
    return sort.direction === 'asc' ? result : -result;
  });
}

export function summarizeUsers(users: AdministrationUser[]) {
  return {
    total: users.length,
    activos: users.filter((user) => user.estado === 'Activo').length,
    inactivos: users.filter((user) => user.estado === 'Inactivo').length,
    bloqueados: users.filter((user) => user.estado === 'Bloqueado').length,
    administradores: users.filter((user) => user.rol === 'Administrador').length,
    analistas: users.filter((user) => user.rol === 'Analista Tributario').length,
    supervisores: users.filter((user) => user.rol === 'Supervisor').length,
  };
}

export function formatDate(value: string | null) {
  if (!value) return 'Sin acceso registrado';
  return new Intl.DateTimeFormat('es-CL').format(new Date(`${value}T12:00:00`));
}

export function createUserId(users: AdministrationUser[]) {
  const next = users.length + 1;
  return `usr-demo-${String(next).padStart(3, '0')}`;
}

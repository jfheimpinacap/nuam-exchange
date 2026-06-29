import type { UserRole } from './session';
import type { PaginationState, SortDirection } from './classification';

export type UserAccountStatus = 'Activo' | 'Inactivo' | 'Bloqueado';

export interface AdministrationUser {
  id: string;
  nombre: string;
  email: string;
  rol: UserRole;
  estado: UserAccountStatus;
  fechaCreacion: string;
  ultimoAcceso: string | null;
  creadoPor: string;
}

export interface UserFormValues {
  nombre: string;
  email: string;
  rol: UserRole;
  estado: UserAccountStatus;
}

export type UserFormErrors = Partial<Record<keyof UserFormValues, string>>;

export interface UserFilters {
  texto: string;
  rol: UserRole | 'Todos';
  estado: UserAccountStatus | 'Todos';
}

export type UserSortKey = 'nombre' | 'email' | 'rol' | 'estado' | 'fechaCreacion' | 'ultimoAcceso';

export interface UserSortState {
  key: UserSortKey;
  direction: SortDirection;
}

export type UserPaginationState = PaginationState;
export type DemoViewState = 'normal' | 'loading' | 'error';

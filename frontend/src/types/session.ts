export type UserRole = 'Administrador' | 'Analista Tributario' | 'Supervisor';

export interface MockUser {
  id: string;
  nombre: string;
  email: string;
  rol: UserRole;
}

export type AuthStatus = 'authenticated' | 'anonymous';

export interface SessionContextValue {
  user: MockUser | null;
  status: AuthStatus;
  isAuthenticated: boolean;
  login: (user: MockUser) => void;
  logout: () => void;
}

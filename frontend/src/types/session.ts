export type UserRole = 'Administrador' | 'Analista Tributario' | 'Supervisor';

export interface MockUser {
  id: string;
  nombre: string;
  email: string;
  rol: UserRole;
}

export type SessionUser = MockUser;

export type AuthStatus = 'restoring' | 'authenticated' | 'anonymous';

export interface ApiLoginCredentials {
  email: string;
  password: string;
}

export interface ApiLoginResult {
  ok: boolean;
  error?: string;
}

export interface SessionContextValue {
  user: SessionUser | null;
  status: AuthStatus;
  isAuthenticated: boolean;
  login: (user: MockUser) => void;
  loginWithApi: (credentials: ApiLoginCredentials) => Promise<ApiLoginResult>;
  logout: () => void;
}

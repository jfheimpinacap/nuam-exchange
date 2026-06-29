import type { SessionUser, UserRole } from '../../types';
export interface SessionContextValue { user: SessionUser | null; isAuthenticated: boolean; login: (role: UserRole, password: string) => boolean; logout: () => void; hasRole: (roles: UserRole[]) => boolean; }

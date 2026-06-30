import { useMemo, useState, type ReactNode } from 'react';
import { clearAccessToken, setAccessToken } from '../../api/auth/accessTokenStore';
import { useApiServices } from '../../api/context/useApiServices';
import { SessionContext } from './SessionContext';
import type { ApiLoginCredentials, ApiLoginResult, MockUser, SessionContextValue, UserRole } from '../../types/session';

const allowedRoles: UserRole[] = ['Administrador', 'Analista Tributario', 'Supervisor'];
const loginErrorMessage = 'No fue posible iniciar sesión. Verifica tus credenciales o la disponibilidad del servicio.';

function isUserRole(value: string): value is UserRole {
  return allowedRoles.some((role) => role === value);
}

interface SessionProviderProps {
  children: ReactNode;
}

export function SessionProvider({ children }: SessionProviderProps) {
  const { authService } = useApiServices();
  const [user, setUser] = useState<MockUser | null>(null);

  const value = useMemo<SessionContextValue>(() => ({
    user,
    status: user ? 'authenticated' : 'anonymous',
    isAuthenticated: Boolean(user),
    login: (selectedUser) => setUser(selectedUser),
    loginWithApi: async (credentials: ApiLoginCredentials): Promise<ApiLoginResult> => {
      clearAccessToken();
      setUser(null);

      try {
        const loginResponse = await authService.login({
          email: credentials.email,
          password: credentials.password,
        });
        const normalizedToken = loginResponse.accessToken?.trim();

        if (!normalizedToken) {
          throw new Error('Missing access token.');
        }

        setAccessToken(normalizedToken);

        const currentUser = await authService.getCurrentUser();

        if (!currentUser.isActive || !isUserRole(currentUser.role)) {
          throw new Error('Invalid user session.');
        }

        setUser({
          id: String(currentUser.id),
          nombre: currentUser.fullName,
          email: currentUser.email,
          rol: currentUser.role,
        });

        return { ok: true };
      } catch {
        clearAccessToken();
        setUser(null);

        return { ok: false, error: loginErrorMessage };
      }
    },
    logout: () => {
      clearAccessToken();
      setUser(null);
    },
  }), [authService, user]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

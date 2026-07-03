import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { clearAccessToken, getAccessToken, setAccessToken } from '../../api/auth/accessTokenStore';
import { isApiError } from '../../api/client/ApiError';
import { useApiServices } from '../../api/context/useApiServices';
import { SessionContext } from './SessionContext';
import type { ApiLoginCredentials, ApiLoginResult, AuthStatus, MockUser, SessionContextValue, UserRole } from '../../types/session';
import type { CurrentUserResponseDto } from '../../api/contracts/auth';

const allowedRoles: UserRole[] = ['Administrador', 'Analista Tributario', 'Supervisor'];
const loginErrorMessage = 'No fue posible iniciar sesión. Verifica tus credenciales o la disponibilidad del servicio.';

function isUserRole(value: string): value is UserRole {
  return allowedRoles.some((role) => role === value);
}

function mapCurrentUser(currentUser: CurrentUserResponseDto): MockUser | null {
  if (!currentUser.isActive || !isUserRole(currentUser.role)) {
    return null;
  }

  return {
    id: String(currentUser.id),
    nombre: currentUser.fullName,
    email: currentUser.email,
    rol: currentUser.role,
  };
}

function isInvalidSessionError(error: unknown): boolean {
  return isApiError(error) && (error.status === 401 || error.status === 403);
}

interface SessionProviderProps {
  children: ReactNode;
}

export function SessionProvider({ children }: SessionProviderProps) {
  const { authService, isApi } = useApiServices();
  const [user, setUser] = useState<MockUser | null>(null);
  const [status, setStatus] = useState<AuthStatus>(() => (
    isApi && getAccessToken() ? 'restoring' : 'anonymous'
  ));

  useEffect(() => {
    if (!isApi) {
      setStatus((currentStatus) => (currentStatus === 'restoring' ? 'anonymous' : currentStatus));
      return;
    }

    if (!getAccessToken()) {
      setUser(null);
      setStatus('anonymous');
      return;
    }

    let isMounted = true;

    const restoreSession = async () => {
      setStatus('restoring');

      try {
        const currentUser = await authService.getCurrentUser();
        const restoredUser = mapCurrentUser(currentUser);

        if (!restoredUser) {
          clearAccessToken();
          if (isMounted) {
            setUser(null);
            setStatus('anonymous');
          }
          return;
        }

        if (isMounted) {
          setUser(restoredUser);
          setStatus('authenticated');
        }
      } catch (error) {
        if (isInvalidSessionError(error)) {
          clearAccessToken();
        }

        if (isMounted) {
          setUser(null);
          setStatus('anonymous');
        }
      }
    };

    void restoreSession();

    return () => {
      isMounted = false;
    };
  }, [authService, isApi]);

  const value = useMemo<SessionContextValue>(() => ({
    user,
    status,
    isAuthenticated: status === 'authenticated' && Boolean(user),
    login: (selectedUser) => {
      setUser(selectedUser);
      setStatus('authenticated');
    },
    loginWithApi: async (credentials: ApiLoginCredentials): Promise<ApiLoginResult> => {
      clearAccessToken();
      setUser(null);
      setStatus('anonymous');

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
        const authenticatedUser = mapCurrentUser(currentUser);

        if (!authenticatedUser) {
          throw new Error('Invalid user session.');
        }

        setUser(authenticatedUser);
        setStatus('authenticated');

        return { ok: true };
      } catch {
        clearAccessToken();
        setUser(null);
        setStatus('anonymous');

        return { ok: false, error: loginErrorMessage };
      }
    },
    logout: () => {
      clearAccessToken();
      setUser(null);
      setStatus('anonymous');
    },
  }), [authService, status, user]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

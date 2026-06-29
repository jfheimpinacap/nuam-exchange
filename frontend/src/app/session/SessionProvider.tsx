import { useMemo, useState, type ReactNode } from 'react';
import { SessionContext } from './SessionContext';
import type { MockUser, SessionContextValue } from '../../types/session';

interface SessionProviderProps {
  children: ReactNode;
}

export function SessionProvider({ children }: SessionProviderProps) {
  const [user, setUser] = useState<MockUser | null>(null);

  const value = useMemo<SessionContextValue>(() => ({
    user,
    status: user ? 'authenticated' : 'anonymous',
    isAuthenticated: Boolean(user),
    login: (selectedUser) => setUser(selectedUser),
    logout: () => setUser(null),
  }), [user]);

  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

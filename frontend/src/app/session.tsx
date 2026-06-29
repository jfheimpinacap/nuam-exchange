import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import type { Role } from '../types';

interface SessionContextValue { role: Role; setRole: (role: Role) => void; userName: string; }
const SessionContext = createContext<SessionContextValue | undefined>(undefined);
export const roles: Role[] = ['Administrador', 'Analista Tributario', 'Supervisor'];

export function SessionProvider({ children }: { children: ReactNode }) {
  const [role, setRole] = useState<Role>('Administrador');
  const value = useMemo(() => ({ role, setRole, userName: `${role.toLowerCase().split(' ').join('.')}.demo` }), [role]);
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>;
}

export function useSession() {
  const context = useContext(SessionContext);
  if (!context) throw new Error('useSession must be used inside SessionProvider');
  return context;
}

import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';

export type RoutePath = '/inicio' | '/calificaciones' | '/cargas' | '/reportes' | '/administracion/usuarios' | '/administracion/roles-permisos' | '/auditoria' | '/respaldos';
interface NavigationContextValue { path: RoutePath; navigate: (path: RoutePath) => void; }
const NavigationContext = createContext<NavigationContextValue | undefined>(undefined);

export function NavigationProvider({ children }: { children: ReactNode }) {
  const [path, navigate] = useState<RoutePath>('/inicio');
  const value = useMemo(() => ({ path, navigate }), [path]);
  return <NavigationContext.Provider value={value}>{children}</NavigationContext.Provider>;
}

export function useNavigation() {
  const context = useContext(NavigationContext);
  if (!context) throw new Error('useNavigation must be used inside NavigationProvider');
  return context;
}

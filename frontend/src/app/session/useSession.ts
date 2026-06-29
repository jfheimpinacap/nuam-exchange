import { useContext } from 'react';
import { SessionContext } from './SessionContext';

export function useSession() {
  const context = useContext(SessionContext);

  if (!context) {
    throw new Error('useSession debe utilizarse dentro de SessionProvider.');
  }

  return context;
}

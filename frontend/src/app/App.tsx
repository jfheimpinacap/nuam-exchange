import { NavigationProvider } from './navigation';
import { SessionProvider } from './session';
import { AdminLayout } from '../layouts/AdminLayout';
import { AppRoutes } from '../routes/AppRoutes';

export function App() { return <SessionProvider><NavigationProvider><AdminLayout><AppRoutes /></AdminLayout></NavigationProvider></SessionProvider>; }

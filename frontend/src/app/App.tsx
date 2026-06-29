import { BrowserRouter } from 'react-router-dom';
import { SessionProvider } from './session/SessionProvider';
import { AppRoutes } from '../routes/AppRoutes';
import { ApiServicesProvider } from '../api/context/ApiServicesProvider';

export function App() {
  return (
    <BrowserRouter>
      <ApiServicesProvider>
        <SessionProvider>
          <AppRoutes />
        </SessionProvider>
      </ApiServicesProvider>
    </BrowserRouter>
  );
}

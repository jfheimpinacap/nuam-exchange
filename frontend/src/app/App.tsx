import { BrowserRouter } from 'react-router-dom';
import { ApiServicesProvider } from '../api/context/ApiServicesProvider';
import { AppRoutes } from '../routes/AppRoutes';
import { SessionProvider } from './session/SessionProvider';
export function App() { return <BrowserRouter><ApiServicesProvider><SessionProvider><AppRoutes /></SessionProvider></ApiServicesProvider></BrowserRouter>; }

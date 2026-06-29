import { useApiServices } from '../api/context/useApiServices';
export function DataSourceIndicator() { const { isApi, apiBaseUrl } = useApiServices(); return <span className="data-source-indicator">Fuente de datos: {isApi ? `API (${apiBaseUrl})` : 'Simulación'}</span>; }

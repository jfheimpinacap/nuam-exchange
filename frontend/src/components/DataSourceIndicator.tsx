import { apiConfig } from '../api/config/apiConfig';
export function DataSourceIndicator() { return <span className="badge">{apiConfig.dataSource === 'mock' ? 'Modo mock' : `API ${apiConfig.baseUrl}`}</span>; }

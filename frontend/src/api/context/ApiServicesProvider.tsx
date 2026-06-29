import { useMemo, type ReactNode } from 'react';
import { apiConfig } from '../config/apiConfig';
import { HttpClient } from '../client/HttpClient';
import { HttpClassificationsService } from '../services/HttpClassificationsService';
import { MockClassificationsService } from '../services/MockClassificationsService';
import { ApiServicesContext } from './ApiServicesContext';
export function ApiServicesProvider({ children }: { children: ReactNode }) {
  const value = useMemo(() => {
    const http = new HttpClient({ baseUrl: apiConfig.baseUrl, timeoutMs: apiConfig.timeoutMs });
    const classificationsService = apiConfig.isApi ? new HttpClassificationsService(http) : new MockClassificationsService();
    return { dataSource: apiConfig.dataSource, classificationsService, apiBaseUrl: apiConfig.baseUrl, isMock: apiConfig.isMock, isApi: apiConfig.isApi };
  }, []);
  return <ApiServicesContext.Provider value={value}>{children}</ApiServicesContext.Provider>;
}

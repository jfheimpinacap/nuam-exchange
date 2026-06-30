import { useMemo, type ReactNode } from 'react';
import { getAccessToken } from '../auth/accessTokenStore';
import { apiConfig } from '../config/apiConfig';
import { HttpClient } from '../client/HttpClient';
import { HttpAuthService } from '../services/HttpAuthService';
import { HttpClassificationsService } from '../services/HttpClassificationsService';
import { MockClassificationsService } from '../services/MockClassificationsService';
import { HttpTaxClassificationsReadService } from '../services/HttpTaxClassificationsReadService';
import { MockTaxClassificationsReadService } from '../services/MockTaxClassificationsReadService';
import { HttpTaxClassificationsWriteService } from '../services/HttpTaxClassificationsWriteService';
import { ApiServicesContext } from './ApiServicesContext';

export function ApiServicesProvider({ children }: { children: ReactNode }) {
  const value = useMemo(() => {
    const http = new HttpClient({
      baseUrl: apiConfig.baseUrl,
      timeoutMs: apiConfig.timeoutMs,
      getAccessToken,
    });
    const authService = new HttpAuthService(http);
    const classificationsService = apiConfig.isApi ? new HttpClassificationsService(http) : new MockClassificationsService();
    const taxClassificationsReadService = apiConfig.isApi ? new HttpTaxClassificationsReadService(http) : new MockTaxClassificationsReadService();
    const taxClassificationsWriteService = apiConfig.isApi ? new HttpTaxClassificationsWriteService(http) : null;

    return {
      dataSource: apiConfig.dataSource,
      classificationsService,
      taxClassificationsReadService,
      taxClassificationsWriteService,
      authService,
      apiBaseUrl: apiConfig.baseUrl,
      isMock: apiConfig.isMock,
      isApi: apiConfig.isApi,
    };
  }, []);

  return <ApiServicesContext.Provider value={value}>{children}</ApiServicesContext.Provider>;
}

import { useMemo, type ReactNode } from "react";
import { getAccessToken } from "../auth/accessTokenStore";
import { apiConfig } from "../config/apiConfig";
import { HttpClient } from "../client/HttpClient";
import { HttpAuthService } from "../services/HttpAuthService";
import { HttpClassificationsService } from "../services/HttpClassificationsService";
import { MockClassificationsService } from "../services/MockClassificationsService";
import { HttpTaxClassificationsReadService } from "../services/HttpTaxClassificationsReadService";
import { MockTaxClassificationsReadService } from "../services/MockTaxClassificationsReadService";
import { HttpTaxClassificationsWriteService } from "../services/HttpTaxClassificationsWriteService";
import { HttpTaxClassificationsSupervisionService } from "../services/HttpTaxClassificationsSupervisionService";
import { HttpTaxClassificationsHistoryService } from "../services/HttpTaxClassificationsHistoryService";
import { HttpTaxClassificationsBulkLoadService } from "../services/HttpTaxClassificationsBulkLoadService";
import { ApiServicesContext } from "./ApiServicesContext";
import { HttpAdministrationService } from "../services/HttpAdministrationService";

export function ApiServicesProvider({ children }: { children: ReactNode }) {
  const value = useMemo(() => {
    const http = new HttpClient({
      baseUrl: apiConfig.baseUrl,
      timeoutMs: apiConfig.timeoutMs,
      getAccessToken,
    });
    const authService = new HttpAuthService(http);
    const classificationsService = apiConfig.isApi
      ? new HttpClassificationsService(http)
      : new MockClassificationsService();
    const taxClassificationsReadService = apiConfig.isApi
      ? new HttpTaxClassificationsReadService(http)
      : new MockTaxClassificationsReadService();
    const taxClassificationsWriteService = apiConfig.isApi
      ? new HttpTaxClassificationsWriteService(http)
      : null;
    const taxClassificationsSupervisionService = apiConfig.isApi
      ? new HttpTaxClassificationsSupervisionService(http)
      : null;
    const taxClassificationsHistoryService = apiConfig.isApi
      ? new HttpTaxClassificationsHistoryService(http)
      : null;
    const taxClassificationsBulkLoadService = apiConfig.isApi
      ? new HttpTaxClassificationsBulkLoadService(http)
      : null;
    const administrationService = apiConfig.isApi ? new HttpAdministrationService(http) : null;

    return {
      dataSource: apiConfig.dataSource,
      classificationsService,
      taxClassificationsReadService,
      taxClassificationsWriteService,
      taxClassificationsSupervisionService,
      taxClassificationsHistoryService,
      taxClassificationsBulkLoadService,
      administrationService,
      authService,
      apiBaseUrl: apiConfig.baseUrl,
      isMock: apiConfig.isMock,
      isApi: apiConfig.isApi,
    };
  }, []);

  return (
    <ApiServicesContext.Provider value={value}>
      {children}
    </ApiServicesContext.Provider>
  );
}

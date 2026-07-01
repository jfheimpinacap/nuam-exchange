import { createContext } from "react";
import type { ClassificationsService } from "../services/ClassificationsService";
import type { AuthService } from "../services/AuthService";
import type { TaxClassificationsReadService } from "../services/TaxClassificationsReadService";
import type { TaxClassificationsWriteService } from "../services/TaxClassificationsWriteService";
import type { TaxClassificationsSupervisionService } from "../services/TaxClassificationsSupervisionService";
import type { TaxClassificationsHistoryService } from "../services/TaxClassificationsHistoryService";

export interface ApiServicesContextValue {
  dataSource: "mock" | "api";
  classificationsService: ClassificationsService;
  taxClassificationsReadService: TaxClassificationsReadService;
  taxClassificationsWriteService: TaxClassificationsWriteService | null;
  taxClassificationsSupervisionService: TaxClassificationsSupervisionService | null;
  taxClassificationsHistoryService: TaxClassificationsHistoryService | null;
  authService: AuthService;
  apiBaseUrl: string;
  isMock: boolean;
  isApi: boolean;
}

export const ApiServicesContext = createContext<ApiServicesContextValue | null>(
  null,
);

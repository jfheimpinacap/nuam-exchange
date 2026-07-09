import { createContext } from "react";
import type { ClassificationsService } from "../services/ClassificationsService";
import type { AuthService } from "../services/AuthService";
import type { TaxClassificationsReadService } from "../services/TaxClassificationsReadService";
import type { TaxClassificationsWriteService } from "../services/TaxClassificationsWriteService";
import type { TaxClassificationsSupervisionService } from "../services/TaxClassificationsSupervisionService";
import type { TaxClassificationsHistoryService } from "../services/TaxClassificationsHistoryService";
import type { TaxClassificationsBulkLoadService } from "../services/TaxClassificationsBulkLoadService";
import type { AdministrationService } from "../services/AdministrationService";
import type { DocumentReviewService } from "../services/DocumentReviewService";

export interface ApiServicesContextValue {
  dataSource: "mock" | "api";
  classificationsService: ClassificationsService;
  taxClassificationsReadService: TaxClassificationsReadService;
  taxClassificationsWriteService: TaxClassificationsWriteService | null;
  taxClassificationsSupervisionService: TaxClassificationsSupervisionService | null;
  taxClassificationsHistoryService: TaxClassificationsHistoryService | null;
  taxClassificationsBulkLoadService: TaxClassificationsBulkLoadService | null;
  authService: AuthService;
  administrationService: AdministrationService | null;
  documentReviewService: DocumentReviewService | null;
  apiBaseUrl: string;
  isMock: boolean;
  isApi: boolean;
}

export const ApiServicesContext = createContext<ApiServicesContextValue | null>(
  null,
);

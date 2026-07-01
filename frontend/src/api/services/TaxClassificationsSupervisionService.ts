import type { TaxClassificationDetailDto } from "../contracts/taxClassificationsRead";
import type { TaxClassificationSupervisorValidationRequestDto } from "../contracts/taxClassificationsSupervision";

export interface TaxClassificationsSupervisionService {
  validate(
    id: number,
    request: TaxClassificationSupervisorValidationRequestDto,
    signal?: AbortSignal,
  ): Promise<TaxClassificationDetailDto>;
}

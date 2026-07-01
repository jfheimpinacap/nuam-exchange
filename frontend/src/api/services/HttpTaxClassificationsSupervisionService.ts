import type { HttpClient } from "../client/HttpClient";
import type { TaxClassificationDetailDto } from "../contracts/taxClassificationsRead";
import type { TaxClassificationSupervisorValidationRequestDto } from "../contracts/taxClassificationsSupervision";
import { parseTaxClassificationDetail } from "./HttpTaxClassificationsReadService";
import type { TaxClassificationsSupervisionService } from "./TaxClassificationsSupervisionService";

export class HttpTaxClassificationsSupervisionService implements TaxClassificationsSupervisionService {
  constructor(private readonly http: HttpClient) {}

  async validate(
    id: number,
    request: TaxClassificationSupervisorValidationRequestDto,
    signal?: AbortSignal,
  ): Promise<TaxClassificationDetailDto> {
    return parseTaxClassificationDetail(
      await this.http.post<unknown>(
        `/tax-classifications/${encodeURIComponent(String(id))}/supervisor-validation`,
        request,
        { signal },
      ),
    );
  }
}

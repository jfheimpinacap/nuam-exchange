import type { TaxClassificationDetailDto } from '../../api/contracts/taxClassificationsRead';
import type { TaxClassificationWriteRequestDto } from '../../api/contracts/taxClassificationsWrite';

export type TaxClassificationApiFormValues = Record<keyof TaxClassificationWriteRequestDto, string>;

export function emptyTaxClassificationApiValues(): TaxClassificationApiFormValues {
  return { market: '', instrumentCode: '', instrumentName: '', classificationType: '', description: '', updatePercentage: '', appliedFactor: '', referenceAmount: '', currency: 'CLP', taxPeriod: String(new Date().getFullYear()), validFrom: '', validTo: '' };
}

export function valuesFromTaxClassificationDetail(detail: TaxClassificationDetailDto): TaxClassificationApiFormValues {
  return { market: detail.market, instrumentCode: detail.instrumentCode ?? '', instrumentName: detail.instrumentName ?? '', classificationType: detail.classificationType, description: detail.description ?? '', updatePercentage: detail.updatePercentage === null ? '' : String(detail.updatePercentage), appliedFactor: detail.appliedFactor === null ? '' : String(detail.appliedFactor), referenceAmount: detail.referenceAmount === null ? '' : String(detail.referenceAmount), currency: detail.currency || 'CLP', taxPeriod: String(detail.taxPeriod), validFrom: detail.validFrom.slice(0, 10), validTo: detail.validTo?.slice(0, 10) ?? '' };
}

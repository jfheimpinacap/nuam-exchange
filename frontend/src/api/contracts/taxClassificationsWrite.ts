export interface TaxClassificationWriteRequestDto {
  market: string;
  instrumentCode: string | null;
  instrumentName: string | null;
  classificationType: string;
  description: string | null;
  updatePercentage: number | null;
  appliedFactor: number | null;
  referenceAmount: number | null;
  currency: string | null;
  taxPeriod: number;
  validFrom: string;
  validTo: string | null;
}

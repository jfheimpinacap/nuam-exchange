export interface TaxClassificationHistoryDto {
  id: number;
  taxClassificationId: number;
  userId: number;
  changeType: string;
  modifiedField: string | null;
  previousValue: string | null;
  newValue: string | null;
  observation: string | null;
  changedAt: string;
}

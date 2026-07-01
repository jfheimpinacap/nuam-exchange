export type TaxClassificationSupervisorDecisionDto =
  | "VALIDADO"
  | "OBSERVADO"
  | "APROBADO";

export interface TaxClassificationSupervisorValidationRequestDto {
  decision: TaxClassificationSupervisorDecisionDto;
  observation?: string;
}

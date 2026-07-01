export interface BulkLoadXFactorErrorDto {
  rowNumber: number;
  code: string;
  message: string;
}

export interface BulkLoadXFactorResultDto {
  uploadId: number;
  totalRows: number;
  successfulRows: number;
  failedRows: number;
  updatedTaxClassificationIds: number[];
  errors: BulkLoadXFactorErrorDto[];
}

export interface TaxClassificationsBulkLoadService {
  uploadXFactor(file: File, signal?: AbortSignal): Promise<BulkLoadXFactorResultDto>;
}

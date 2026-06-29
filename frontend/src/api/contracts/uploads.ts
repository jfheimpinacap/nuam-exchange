export interface UploadRowErrorDto { rowNumber: number; field?: string; message: string; }
export interface UploadValidationResponseDto { uploadId: string; isValid: boolean; totalRows: number; validRows: number; errors: UploadRowErrorDto[]; }
export interface UploadProcessResponseDto { processId: string; processedRows: number; createdRows: number; updatedRows: number; message: string; }
export interface UploadHistoryItemDto { id: string; type: 'x-factor' | 'x-amount'; fileName: string; uploadedAt: string; uploadedBy: string; status: string; totalRows: number; }

export type UploadType = 'x-factor' | 'x-monto';
export type UploadStage = 'idle' | 'reading' | 'ready' | 'processing' | 'completed' | 'file-error';
export type UploadRowStatus = 'valid' | 'invalid';
export interface UploadFileMetadata { name: string; size: number; type: string; lastModified: number; }
export interface XFactorValues { ejercicio: number; mercado: string; instrumento: string; fechaPago: string; secuenciaEvento: string; factorActualizacion: number; }
export interface XAmountValues { ejercicio: number; mercado: string; instrumento: string; fechaPago: string; secuenciaEvento: string; monto: number; }
export type UploadNormalizedValues = Partial<XFactorValues & XAmountValues>;
export interface UploadValidationError { rowNumber: number; column: string; code: string; message: string; value?: string; }
export interface UploadParsedRow { rowNumber: number; rawValues: Record<string, string>; normalizedValues: UploadNormalizedValues; status: UploadRowStatus; errors: UploadValidationError[]; }
export interface UploadResult { totalRows: number; validRows: number; invalidRows: number; processedRows: number; rejectedRows: number; startedAt: string; completedAt: string; }
export interface UploadParseResult { delimiter: ',' | ';'; headers: string[]; rows: string[][]; structureErrors: UploadValidationError[]; }
export interface UploadReviewItem { id: string; type: UploadType; fileName: string; date: string; owner: string; totalRows: number; validRows: number; invalidRows: number; status: 'Procesada con observaciones' | 'Requiere revisión'; inconsistencies: Array<UploadValidationError & { severity: 'Advertencia' | 'Error' }>; }

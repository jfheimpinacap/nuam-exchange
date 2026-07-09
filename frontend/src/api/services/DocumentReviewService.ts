export type PdfReviewStatus = 'VALID' | 'INCOMPLETE' | 'UNSUPPORTED' | 'INVALID_FILE';

export interface PdfDocumentReviewResultDto {
  reviewId: string | null;
  fileName: string;
  fileSizeBytes: number;
  pageCount: number;
  status: PdfReviewStatus;
  message: string;
  detectedFields: Record<string, string>;
  missingFields: string[];
  warnings: string[];
  textPreview: string;
}

export interface DocumentReviewService {
  reviewPdf(file: File, signal?: AbortSignal): Promise<PdfDocumentReviewResultDto>;
}

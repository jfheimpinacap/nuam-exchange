import type { SortDirection } from './common';
export interface ClassificationReportRequestDto { from?: string; to?: string; market?: string; status?: string; page: number; pageSize: number; sortDirection?: SortDirection; }
export interface UploadReportRequestDto { from?: string; to?: string; type?: 'x-factor' | 'x-amount'; page: number; pageSize: number; }
export interface ReportExportRequestDto { report: 'classifications' | 'uploads'; format: 'csv' | 'xlsx' | 'pdf'; filters: Record<string, string | number | boolean | undefined>; }
export interface ReportExportResponseDto { exportId: string; fileName: string; downloadUrl: string; expiresAt: string; }

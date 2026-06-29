import type { ClassificationSortKey, SortDirection } from './classification';
export type ReportType = 'classifications' | 'uploads';
export interface ClassificationReportFilters { ejercicio: string; mercado: string; origen: string; estado: string; fechaDesde: string; fechaHasta: string; texto: string; }
export interface UploadReportFilters { tipo: string; estado: string; responsable: string; fechaDesde: string; fechaHasta: string; }
export interface ReportSortState { key: ClassificationSortKey | 'fileName' | 'type' | 'date' | 'owner' | 'totalRows' | 'validRows' | 'invalidRows' | 'status'; direction: SortDirection; }

import type { TaxClassification } from '../../types';
export interface ClassificationFilters { text?: string; market?: string; source?: string; fiscalYear?: string; status?: string; page: number; pageSize: number; sortBy?: keyof TaxClassification; sortDirection?: 'asc' | 'desc'; }
export interface ClassificationCatalogs { markets: string[]; sources: string[]; fiscalYears: number[]; statuses: string[]; }

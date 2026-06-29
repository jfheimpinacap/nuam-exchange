import type { PaginatedResponse, SortDirection } from './common';
export type ClassificationStatusDto = 'Vigente' | 'Pendiente' | 'Observada' | 'Rechazada';
export type ClassificationSortByDto = 'fiscalYear' | 'instrument' | 'paymentDate' | 'amount' | 'status';
export interface ClassificationDto { id: string; market: string; source: string; fiscalYear: number; instrument: string; paymentDate: string; description: string; eventSequence: string; updateFactor: number; amount: number; status: ClassificationStatusDto; rowVersion?: string; }
export interface ClassificationListRequestDto { page: number; pageSize: number; search?: string; market?: string; source?: string; fiscalYear?: number; status?: ClassificationStatusDto; sortBy: ClassificationSortByDto; sortDirection: SortDirection; }
export interface ClassificationCatalogsDto { markets: string[]; sources: string[]; fiscalYears: number[]; statuses: ClassificationStatusDto[]; }
export type ClassificationListResponseDto = PaginatedResponse<ClassificationDto>;
export type CreateClassificationRequestDto = Omit<ClassificationDto, 'id' | 'rowVersion'>;
export type UpdateClassificationRequestDto = Omit<ClassificationDto, 'id' | 'rowVersion'>;
export interface CopyClassificationRequestDto { paymentDate?: string; description?: string; eventSequence?: string; }
export interface ClassificationWriteResponseDto { classification: ClassificationDto; message: string; }

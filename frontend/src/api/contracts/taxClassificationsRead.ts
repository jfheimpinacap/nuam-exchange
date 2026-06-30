import type { SortDirection } from './common';

export type TaxClassificationSortByDto = 'id' | 'market' | 'instrumentCode' | 'instrumentName' | 'classificationType' | 'currency' | 'taxPeriod' | 'validFrom' | 'validTo' | 'status' | 'createdAt' | 'updatedAt';

export interface TaxClassificationListRequestDto {
  page: number;
  pageSize: number;
  search?: string;
  market?: string;
  exercise?: number;
  status?: string;
  sortBy: TaxClassificationSortByDto;
  sortDirection: SortDirection;
}

export interface TaxClassificationReadDto {
  id: number;
  market: string;
  instrumentCode: string | null;
  instrumentName: string | null;
  classificationType: string;
  description: string | null;
  updatePercentage: number | null;
  appliedFactor: number | null;
  referenceAmount: number | null;
  currency: string;
  taxPeriod: number;
  validFrom: string;
  validTo: string | null;
  status: string;
}

export interface TaxClassificationDetailDto extends TaxClassificationReadDto {
  createdAt?: string;
  updatedAt?: string;
  creatorUserId?: string | number | null;
}

export interface TaxClassificationListResponseDto {
  items: TaxClassificationReadDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface TaxClassificationFilterOptionsDto {
  markets: string[];
  exercises: number[];
  statuses: string[];
}

export type TaxClassificationUiSortKey = 'taxPeriod' | 'instrumentCode' | 'validFrom' | 'status' | 'market';
export interface TaxClassificationSortState { key: TaxClassificationUiSortKey; direction: SortDirection; }

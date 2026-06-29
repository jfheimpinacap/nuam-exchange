export type ClassificationStatus = 'Vigente' | 'Pendiente' | 'Observada' | 'Rechazada';

export interface Classification {
  id: string;
  mercado: string;
  origen: string;
  ejercicio: number;
  instrumento: string;
  fechaPago: string;
  descripcion: string;
  secuenciaEvento: string;
  factorActualizacion: number;
  monto: number;
  estado: ClassificationStatus;
}

export interface ClassificationFilters {
  mercado: string;
  origen: string;
  ejercicio: string;
  estado: string;
  texto: string;
}

export type SortDirection = 'asc' | 'desc';
export type ClassificationSortKey = 'ejercicio' | 'instrumento' | 'fechaPago' | 'monto' | 'estado';

export interface SortState {
  key: ClassificationSortKey;
  direction: SortDirection;
}

export interface PaginationState {
  page: number;
  pageSize: 5 | 10 | 20 | 50;
}

import type { ClassificationStatus } from './classification';
export type DashboardYearFilter = 'Todos' | '2024' | '2025' | '2026';
export type DashboardMarketFilter = 'Todos' | 'Acciones' | 'Renta Fija' | 'Fondos';
export type DashboardDemoState = 'normal' | 'loading' | 'empty' | 'error';
export interface DashboardFilters { year: DashboardYearFilter; market: DashboardMarketFilter; }
export interface DashboardMetrics { total: number; vigente: number; pendiente: number; observada: number; montoTotal: number; cargas: number; filasConError: number; }
export interface ChartItem { label: string; value: number; formattedValue?: string; }
export interface DashboardActivity { id: string; date: string; type: string; description: string; owner: string; status: ClassificationStatus | string; actionLabel: string; actionPath: string; }

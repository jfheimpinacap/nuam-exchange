export interface DashboardSummaryRequestDto { from?: string; to?: string; market?: string; }
export interface DashboardMetricDto { key: string; label: string; value: number; variation?: number; }
export interface DashboardSeriesDto { name: string; points: { label: string; value: number }[]; }
export interface DashboardSummaryResponseDto { metrics: DashboardMetricDto[]; series: DashboardSeriesDto[]; generatedAt: string; }

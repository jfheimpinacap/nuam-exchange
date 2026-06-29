import type { DashboardActivity } from '../types/dashboard';
export const dashboardActivity: DashboardActivity[] = [
  { id: 'act-review-001', date: '2026-06-20 11:15', type: 'Revisión de cargas', description: 'Supervisor revisó inconsistencias de cargas del mes.', owner: 'Supervisor Demo', status: 'Observada', actionLabel: 'Ver Reportes', actionPath: '/reportes' },
  { id: 'act-report-001', date: '2026-06-18 16:30', type: 'Reporte', description: 'Consulta consolidada de calificaciones y cargas.', owner: 'Analista Demo', status: 'Vigente', actionLabel: 'Ver Reportes', actionPath: '/reportes' },
];

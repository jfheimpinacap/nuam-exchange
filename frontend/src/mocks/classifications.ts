import type { AuditEvent, Classification } from '../types';

export const mockClassifications: Classification[] = [
  { id: 'CL-001', market: 'Chile', instrument: 'NUAM-CLP-01', description: 'Dividendo local serie A', fiscalYear: 2026, paymentDate: '2026-03-18', amount: 18500000, status: 'Aprobada' },
  { id: 'CO-014', market: 'Colombia', instrument: 'NUAM-COP-14', description: 'Interés corporativo trimestral', fiscalYear: 2026, paymentDate: '2026-04-07', amount: 32100000, status: 'Pendiente' },
  { id: 'PE-008', market: 'Perú', instrument: 'NUAM-PEN-08', description: 'Evento tributario mercado integrado', fiscalYear: 2025, paymentDate: '2025-12-20', amount: 12650000, status: 'Borrador' },
  { id: 'CL-032', market: 'Chile', instrument: 'NUAM-CLP-32', description: 'Corrección histórica de factor', fiscalYear: 2025, paymentDate: '2025-10-02', amount: 8300000, status: 'Rechazada' }
];

export const mockAuditEvents: AuditEvent[] = [
  { id: 'AUD-1001', date: '2026-06-20 09:14', user: 'admin.demo', module: 'Calificaciones', action: 'Consulta', result: 'OK', severity: 'Info' },
  { id: 'AUD-1002', date: '2026-06-21 15:42', user: 'supervisor.demo', module: 'Cargas', action: 'Revisión', result: 'Observada', severity: 'Advertencia' },
  { id: 'AUD-1003', date: '2026-06-22 11:03', user: 'admin.demo', module: 'Respaldos', action: 'Simulación', result: 'OK', severity: 'Info' }
];

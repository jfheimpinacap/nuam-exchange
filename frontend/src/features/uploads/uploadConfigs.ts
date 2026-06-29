import type { UploadType } from '../../types/upload';
export interface UploadColumnConfig { key: string; label: string; required: boolean; type: string; example: string; rule: string; }
export interface UploadConfig { type: UploadType; title: string; shortTitle: string; description: string; valueColumn: string; valueLabel: string; columns: UploadColumnConfig[]; examples: string[][]; }
const common = [
  { key: 'ejercicio', label: 'ejercicio', required: true, type: 'Entero', example: '2026', rule: 'Permitidos: 2024, 2025, 2026 y 2027.' },
  { key: 'mercado', label: 'mercado', required: true, type: 'Texto', example: 'Acciones', rule: 'Acciones, Renta Fija o Fondos.' },
  { key: 'instrumento', label: 'instrumento', required: true, type: 'Texto', example: 'NUAM-A', rule: 'Entre 2 y 30 caracteres.' },
  { key: 'fechaPago', label: 'fechaPago', required: true, type: 'Fecha', example: '15-03-2026', rule: 'Formato dd-MM-yyyy y fecha real.' },
  { key: 'secuenciaEvento', label: 'secuenciaEvento', required: true, type: 'Texto', example: 'EVT-001', rule: 'Entre 3 y 20 caracteres; letras, números y guion.' },
];
export const uploadConfigs: Record<UploadType, UploadConfig> = {
  'x-factor': { type: 'x-factor', title: 'Carga X Factor', shortTitle: 'X Factor', description: 'Importación simulada de factores de actualización mediante archivo CSV.', valueColumn: 'factorActualizacion', valueLabel: 'Factor', columns: [...common, { key: 'factorActualizacion', label: 'factorActualizacion', required: true, type: 'Decimal', example: '1.034521', rule: 'Número mayor que cero, máximo 6 decimales.' }], examples: [['2026','Acciones','NUAM-A','15-03-2026','EVT-001','1.034521'], ['2025','Fondos','FONDO-CL','20-04-2025','EVT-002','0.987654']] },
  'x-monto': { type: 'x-monto', title: 'Carga X Monto', shortTitle: 'X Monto', description: 'Importación simulada de montos tributarios mediante archivo CSV.', valueColumn: 'monto', valueLabel: 'Monto', columns: [...common, { key: 'monto', label: 'monto', required: true, type: 'Decimal', example: '1500000.50', rule: 'Número mayor que cero, máximo 2 decimales.' }], examples: [['2026','Acciones','NUAM-A','15-03-2026','EVT-001','1500000.50'], ['2025','Renta Fija','BONO-CL','20-04-2025','EVT-002','750000.00']] },
};
export const provisionalFormatMessage = 'Formato provisional del frontend. El contrato definitivo será entregado por el backend.';
export const maxUploadBytes = 5 * 1024 * 1024;
export const maxDataRows = 1000;

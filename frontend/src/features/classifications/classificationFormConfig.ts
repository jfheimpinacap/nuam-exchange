import type { ClassificationStatus } from '../../types/classification';

export type ClassificationFormMode = 'create' | 'edit' | 'copy';
export type ClassificationFormValues = {
  mercado: string;
  origen: string;
  ejercicio: string;
  instrumento: string;
  fechaPago: string;
  descripcion: string;
  secuenciaEvento: string;
  factorActualizacion: string;
  monto: string;
  estado: string;
};
export type ClassificationFormErrors = Partial<Record<keyof ClassificationFormValues, string>>;

export const mercadoOptions = ['Acciones', 'Renta Fija', 'Fondos'] as const;
export const origenOptions = ['Manual', 'Carga X Factor', 'Carga X Monto'] as const;
export const ejercicioOptions = ['2024', '2025', '2026', '2027'] as const;
export const estadoOptions: ClassificationStatus[] = ['Vigente', 'Pendiente', 'Observada', 'Rechazada'];

export const createInitialValues: ClassificationFormValues = {
  mercado: 'Acciones',
  origen: 'Manual',
  ejercicio: '2026',
  instrumento: '',
  fechaPago: '',
  descripcion: '',
  secuenciaEvento: '',
  factorActualizacion: '1',
  monto: '',
  estado: 'Pendiente',
};

export const fieldLabels: Record<keyof ClassificationFormValues, string> = {
  mercado: 'Mercado', origen: 'Origen', ejercicio: 'Ejercicio', instrumento: 'Instrumento', fechaPago: 'Fecha de pago', descripcion: 'Descripción', secuenciaEvento: 'Secuencia de evento', factorActualizacion: 'Factor de actualización', monto: 'Monto', estado: 'Estado',
};

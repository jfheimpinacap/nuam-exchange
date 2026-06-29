import type { Classification } from '../../types/classification';
import { calculateMockUpdateFactor } from './mockFactorCalculator';
import type { ClassificationFormValues } from './classificationFormConfig';

function isValidParts(year: number, month: number, day: number) {
  const date = new Date(year, month - 1, day);
  return date.getFullYear() === year && date.getMonth() === month - 1 && date.getDate() === day;
}

export function ddMmYyyyToHtmlDate(value: string): string {
  const match = /^(\d{2})-(\d{2})-(\d{4})$/.exec(value);
  if (!match) return '';
  const [, dd, mm, yyyy] = match;
  const day = Number(dd); const month = Number(mm); const year = Number(yyyy);
  return isValidParts(year, month, day) ? `${yyyy}-${mm}-${dd}` : '';
}

export function htmlDateToDdMmYyyy(value: string): string {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return '';
  const [, yyyy, mm, dd] = match;
  const year = Number(yyyy); const month = Number(mm); const day = Number(dd);
  return isValidParts(year, month, day) ? `${dd}-${mm}-${yyyy}` : '';
}

export function isValidHtmlDate(value: string): boolean { return htmlDateToDdMmYyyy(value) !== ''; }

export function valuesFromClassification(record: Classification): ClassificationFormValues {
  const fechaPago = ddMmYyyyToHtmlDate(record.fechaPago);
  return {
    mercado: record.mercado,
    origen: record.origen,
    ejercicio: String(record.ejercicio),
    instrumento: record.instrumento,
    fechaPago,
    descripcion: record.descripcion,
    secuenciaEvento: record.secuenciaEvento,
    factorActualizacion: String(calculateMockUpdateFactor(String(record.ejercicio), fechaPago)),
    monto: String(record.monto),
    estado: record.estado,
  };
}

export function copyValuesFromClassification(record: Classification): ClassificationFormValues {
  return { ...valuesFromClassification(record), origen: 'Manual', estado: 'Pendiente', secuenciaEvento: '' };
}

export function normalizeProcessedValues(values: ClassificationFormValues) {
  return { ...values, instrumento: values.instrumento.trim().toUpperCase(), fechaPago: htmlDateToDdMmYyyy(values.fechaPago), monto: Number(values.monto).toLocaleString('es-CL') };
}

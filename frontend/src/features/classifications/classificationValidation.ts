import { ejercicioOptions, estadoOptions, mercadoOptions, origenOptions, type ClassificationFormErrors, type ClassificationFormValues } from './classificationFormConfig';
import { isValidHtmlDate } from './classificationFormUtils';

export function validateClassificationForm(values: ClassificationFormValues): ClassificationFormErrors {
  const errors: ClassificationFormErrors = {};
  if (!(mercadoOptions as readonly string[]).includes(values.mercado)) errors.mercado = 'Seleccione un mercado válido.';
  if (!(origenOptions as readonly string[]).includes(values.origen)) errors.origen = 'Seleccione un origen válido.';
  if (!(ejercicioOptions as readonly string[]).includes(values.ejercicio)) errors.ejercicio = 'Seleccione un ejercicio autorizado.';
  const instrumento = values.instrumento.trim();
  if (!instrumento) errors.instrumento = 'Ingrese un instrumento.'; else if (instrumento.length < 2 || instrumento.length > 30) errors.instrumento = 'Debe tener entre 2 y 30 caracteres.';
  if (!values.fechaPago) errors.fechaPago = 'Ingrese la fecha de pago.'; else if (!isValidHtmlDate(values.fechaPago)) errors.fechaPago = 'Ingrese una fecha válida.';
  const descripcion = values.descripcion.trim();
  if (!descripcion) errors.descripcion = 'Ingrese una descripción.'; else if (descripcion.length < 5 || descripcion.length > 150) errors.descripcion = 'Debe tener entre 5 y 150 caracteres.';
  const secuencia = values.secuenciaEvento.trim();
  if (!secuencia) errors.secuenciaEvento = 'Ingrese una secuencia de evento.'; else if (secuencia.length < 3 || secuencia.length > 20) errors.secuenciaEvento = 'Debe tener entre 3 y 20 caracteres.'; else if (!/^[A-Za-z0-9-]+$/.test(secuencia)) errors.secuenciaEvento = 'Use solo letras, números y guion.';
  if (!values.monto) errors.monto = 'Ingrese el monto.'; else if (Number(values.monto) <= 0) errors.monto = 'El monto debe ser mayor que cero.'; else if (!/^\d+(\.\d{1,2})?$/.test(values.monto)) errors.monto = 'Use máximo dos decimales.';
  if (!(estadoOptions as readonly string[]).includes(values.estado)) errors.estado = 'Seleccione un estado válido.';
  if (Number(values.factorActualizacion) <= 0) errors.factorActualizacion = 'El factor calculado debe ser mayor que cero.';
  return errors;
}

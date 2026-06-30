import { useMemo, useRef, useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserFriendlyApiMessage } from '../../api/client/ApiError';
import type { TaxClassificationWriteRequestDto } from '../../api/contracts/taxClassificationsWrite';
import type { TaxClassificationApiFormValues } from './taxClassificationApiFormValues';
import { useApiServices } from '../../api/context/useApiServices';
import { Button } from '../../components/Button';
import { FormField } from '../../components/FormField';
import { InlineMessage } from '../../components/InlineMessage';

type FormMode = 'create' | 'edit';
type Values = TaxClassificationApiFormValues;
type Errors = Partial<Record<keyof Values, string>>;

const fields: (keyof Values)[] = ['market', 'instrumentCode', 'instrumentName', 'classificationType', 'description', 'updatePercentage', 'appliedFactor', 'referenceAmount', 'currency', 'taxPeriod', 'validFrom', 'validTo'];
const labels: Record<keyof Values, string> = { market: 'Mercado', instrumentCode: 'Código de instrumento', instrumentName: 'Nombre del instrumento', classificationType: 'Tipo de clasificación', description: 'Descripción', updatePercentage: 'Porcentaje de actualización', appliedFactor: 'Factor aplicado', referenceAmount: 'Monto de referencia', currency: 'Moneda', taxPeriod: 'Período tributario', validFrom: 'Vigencia desde', validTo: 'Vigencia hasta' };

function trimmed(value: string) { return value.trim(); }
function optionalString(value: string) { const next = trimmed(value); return next === '' ? null : next; }
function optionalNumber(value: string) { const next = trimmed(value); return next === '' ? null : Number(next); }
function isIsoDate(value: string) { return /^\d{4}-\d{2}-\d{2}$/.test(value); }
function validate(values: Values): Errors {
  const errors: Errors = {};
  if (!trimmed(values.market)) errors.market = 'Ingrese el mercado.'; else if (trimmed(values.market).length > 120) errors.market = 'Máximo 120 caracteres.';
  if (trimmed(values.instrumentCode).length > 80) errors.instrumentCode = 'Máximo 80 caracteres.';
  if (trimmed(values.instrumentName).length > 180) errors.instrumentName = 'Máximo 180 caracteres.';
  if (!trimmed(values.classificationType)) errors.classificationType = 'Ingrese el tipo de clasificación.'; else if (trimmed(values.classificationType).length > 100) errors.classificationType = 'Máximo 100 caracteres.';
  if (trimmed(values.description).length > 500) errors.description = 'Máximo 500 caracteres.';
  if (trimmed(values.currency).length > 10) errors.currency = 'Máximo 10 caracteres.';
  const period = Number(values.taxPeriod);
  if (!trimmed(values.taxPeriod)) errors.taxPeriod = 'Ingrese el período tributario.'; else if (!Number.isInteger(period) || period < 2000 || period > 2100) errors.taxPeriod = 'Debe estar entre 2000 y 2100.';
  if (!trimmed(values.validFrom)) errors.validFrom = 'Ingrese la vigencia desde.'; else if (!isIsoDate(values.validFrom)) errors.validFrom = 'Use una fecha válida.';
  if (trimmed(values.validTo) && !isIsoDate(values.validTo)) errors.validTo = 'Use una fecha válida.';
  if (isIsoDate(values.validFrom) && isIsoDate(values.validTo) && values.validTo < values.validFrom) errors.validTo = 'Debe ser igual o posterior a la vigencia desde.';
  (['updatePercentage', 'appliedFactor', 'referenceAmount'] as const).forEach((field) => { const value = trimmed(values[field]); if (value && (Number.isNaN(Number(value)) || Number(value) < 0)) errors[field] = 'Debe ser un número mayor o igual a 0.'; });
  return errors;
}
function toRequest(values: Values): TaxClassificationWriteRequestDto { return { market: trimmed(values.market), instrumentCode: optionalString(values.instrumentCode), instrumentName: optionalString(values.instrumentName), classificationType: trimmed(values.classificationType), description: optionalString(values.description), updatePercentage: optionalNumber(values.updatePercentage), appliedFactor: optionalNumber(values.appliedFactor), referenceAmount: optionalNumber(values.referenceAmount), currency: optionalString(values.currency), taxPeriod: Number(values.taxPeriod), validFrom: values.validFrom, validTo: optionalString(values.validTo) }; }

interface Props { mode: FormMode; id?: number; initialValues: Values; }
export function TaxClassificationApiForm({ mode, id, initialValues }: Props) {
  const navigate = useNavigate();
  const { taxClassificationsWriteService } = useApiServices();
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<Errors>({});
  const [showSummary, setShowSummary] = useState(false);
  const [saving, setSaving] = useState(false);
  const [apiError, setApiError] = useState('');
  const refs = useRef<Partial<Record<keyof Values, HTMLInputElement | HTMLTextAreaElement>>>({});
  const isDirty = useMemo(() => JSON.stringify(values) !== JSON.stringify(initialValues), [values, initialValues]);
  function update(key: keyof Values) { return (event: ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => { setValues((current) => ({ ...current, [key]: event.target.value })); setErrors((current) => { const next = { ...current }; delete next[key]; return next; }); setShowSummary(false); setApiError(''); }; }
  async function submit(event: FormEvent) { event.preventDefault(); if (saving || !taxClassificationsWriteService) return; const nextErrors = validate(values); setErrors(nextErrors); setShowSummary(Object.keys(nextErrors).length > 0); if (Object.keys(nextErrors).length > 0) { refs.current[fields.find((field) => nextErrors[field]) ?? 'market']?.focus(); return; } setSaving(true); setApiError(''); try { const request = toRequest(values); if (mode === 'create') await taxClassificationsWriteService.create(request); else if (id !== undefined) await taxClassificationsWriteService.update(id, request); navigate('/calificaciones', { state: { taxClassificationWriteSuccess: mode === 'create' ? 'Calificación tributaria creada correctamente.' : 'Calificación tributaria actualizada correctamente.' } }); } catch (error) { setApiError(getUserFriendlyApiMessage(error)); } finally { setSaving(false); } }
  function cancel() { navigate('/calificaciones'); }
  const described = (key: keyof Values) => errors[key] ? `${key}-error` : undefined;
  return <section className="content-card classification-form-page"><header className="page-header"><p className="eyebrow">Calificaciones Tributarias</p><h1>{mode === 'create' ? 'Ingresar Calificación Tributaria' : 'Modificar Calificación Tributaria'}</h1><p>Formulario API real. Los campos marcados con * son obligatorios.</p></header>{apiError ? <InlineMessage tone="error" message={apiError} /> : null}{showSummary ? <div className="error-summary" role="alert" tabIndex={-1}><strong>Revise los campos marcados:</strong><ul>{fields.filter((field) => errors[field]).map((field) => <li key={field}>{labels[field]}: {errors[field]}</li>)}</ul></div> : null}<form className="classification-form" onSubmit={submit} noValidate><fieldset disabled={saving} className="form-section"><legend>Datos reales de escritura</legend><div className="classification-form-grid">{fields.map((field) => <FormField key={field} id={field} label={`${labels[field]}${field === 'market' || field === 'classificationType' || field === 'taxPeriod' || field === 'validFrom' ? ' *' : ''}`}>{field === 'description' ? <textarea ref={(el) => { if (el) refs.current[field] = el; }} id={field} value={values[field]} onChange={update(field)} maxLength={500} aria-invalid={Boolean(errors[field])} aria-describedby={described(field)} /> : <input ref={(el) => { if (el) refs.current[field] = el; }} id={field} value={values[field]} onChange={update(field)} type={field === 'validFrom' || field === 'validTo' ? 'date' : field === 'taxPeriod' || field === 'updatePercentage' || field === 'appliedFactor' || field === 'referenceAmount' ? 'number' : 'text'} min={field === 'taxPeriod' ? 2000 : field === 'updatePercentage' || field === 'appliedFactor' || field === 'referenceAmount' ? 0 : undefined} max={field === 'taxPeriod' ? 2100 : undefined} step={field === 'taxPeriod' ? 1 : 'any'} maxLength={field === 'market' ? 120 : field === 'instrumentCode' ? 80 : field === 'instrumentName' ? 180 : field === 'classificationType' ? 100 : field === 'currency' ? 10 : undefined} aria-invalid={Boolean(errors[field])} aria-describedby={described(field)} />}{errors[field] ? <span id={`${field}-error`} className="field-error">{errors[field]}</span> : null}</FormField>)}</div></fieldset><div className="form-actions"><Button variant="primary" type="submit" disabled={saving}>{saving ? 'Guardando...' : 'Guardar'}</Button><Button type="button" onClick={() => setValues(initialValues)} disabled={saving || !isDirty}>Restablecer</Button><Button type="button" onClick={cancel} disabled={saving}>Cancelar</Button></div></form></section>;
}

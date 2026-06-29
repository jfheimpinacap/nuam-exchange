import { useEffect, useMemo, useRef, useState, type ChangeEvent, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button } from '../../components/Button';
import { FormField } from '../../components/FormField';
import { InlineMessage } from '../../components/InlineMessage';
import { calculateMockUpdateFactor } from './mockFactorCalculator';
import { fieldLabels, estadoOptions, ejercicioOptions, mercadoOptions, origenOptions, type ClassificationFormErrors, type ClassificationFormMode, type ClassificationFormValues } from './classificationFormConfig';
import { normalizeProcessedValues } from './classificationFormUtils';
import { validateClassificationForm } from './classificationValidation';

interface Props { mode: ClassificationFormMode; title: string; initialValues: ClassificationFormValues; infoMessage?: string; submitLabel: string; resetLabel: string; successMessage: string; }
type SubmitState = 'ready' | 'processing' | 'success';
const fields: (keyof ClassificationFormValues)[] = ['mercado', 'origen', 'ejercicio', 'instrumento', 'fechaPago', 'descripcion', 'secuenciaEvento', 'factorActualizacion', 'monto', 'estado'];

export function ClassificationForm({ mode, title, initialValues, infoMessage, submitLabel, resetLabel, successMessage }: Props) {
  const navigate = useNavigate();
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<ClassificationFormErrors>({});
  const [showSummary, setShowSummary] = useState(false);
  const [submitState, setSubmitState] = useState<SubmitState>('ready');
  const [processed, setProcessed] = useState<Record<string, string | number> | null>(null);
  const [showDiscard, setShowDiscard] = useState(false);
  const refs = useRef<Partial<Record<keyof ClassificationFormValues, HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>>>({});
  const timer = useRef<number | null>(null);

  useEffect(() => () => { if (timer.current) window.clearTimeout(timer.current); }, []);
  useEffect(() => setValues((current) => ({ ...current, factorActualizacion: String(calculateMockUpdateFactor(current.ejercicio, current.fechaPago)) })), [values.ejercicio, values.fechaPago]);

  const isDirty = useMemo(() => JSON.stringify(values) !== JSON.stringify(initialValues), [values, initialValues]);
  const isBusy = submitState === 'processing';

  function update<K extends keyof ClassificationFormValues>(key: K, value: ClassificationFormValues[K]) {
    const nextValue = key === 'instrumento' ? value.toUpperCase() : value;
    setValues((current) => ({ ...current, [key]: nextValue }));
    setErrors((current) => { const next = { ...current }; delete next[key]; return next; });
    setShowSummary(false);
    setSubmitState('ready');
  }
  function handleChange(key: keyof ClassificationFormValues) { return (event: ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) => update(key, event.target.value); }
  function focusFirst(nextErrors: ClassificationFormErrors) { const first = fields.find((field) => nextErrors[field]); if (first) refs.current[first]?.focus(); }
  function submit(event: FormEvent) {
    event.preventDefault();
    if (isBusy) return;
    const nextErrors = validateClassificationForm(values);
    setErrors(nextErrors); setShowSummary(Object.keys(nextErrors).length > 0);
    if (Object.keys(nextErrors).length > 0) { window.setTimeout(() => focusFirst(nextErrors)); return; }
    setSubmitState('processing'); setProcessed(null);
    timer.current = window.setTimeout(() => { setSubmitState('success'); setProcessed(normalizeProcessedValues(values)); }, 320);
  }
  function reset() { setValues(initialValues); setErrors({}); setShowSummary(false); setProcessed(null); setSubmitState('ready'); }
  function cancel() { if (isDirty) setShowDiscard(true); else navigate('/calificaciones'); }
  const described = (key: keyof ClassificationFormValues, help?: string) => [errors[key] ? `${key}-error` : '', help ?? ''].filter(Boolean).join(' ') || undefined;

  return <section className="content-card classification-form-page">
    <header className="page-header"><p className="eyebrow">Calificaciones Tributarias</p><h1>{title}</h1><p>Formulario simulado en memoria. No guarda datos ni modifica los registros mockeados.</p></header>
    {infoMessage ? <InlineMessage message={infoMessage} /> : null}
    {showSummary ? <div className="error-summary" role="alert" tabIndex={-1}><strong>Revise los campos marcados:</strong><ul>{fields.filter((field) => errors[field]).map((field) => <li key={field}>{fieldLabels[field]}: {errors[field]}</li>)}</ul></div> : null}
    <form className="classification-form" onSubmit={submit} noValidate>
      <fieldset disabled={isBusy} className="form-section"><legend>Datos de la calificación</legend><div className="classification-form-grid">
        <FormField id="mercado" label="Mercado"><select ref={(el) => { if (el) refs.current.mercado = el; }} id="mercado" value={values.mercado} onChange={handleChange('mercado')} aria-invalid={Boolean(errors.mercado)} aria-describedby={described('mercado')}>{mercadoOptions.map((item) => <option key={item}>{item}</option>)}</select>{errors.mercado ? <span id="mercado-error" className="field-error">{errors.mercado}</span> : null}</FormField>
        <FormField id="origen" label="Origen"><select ref={(el) => { if (el) refs.current.origen = el; }} id="origen" value={values.origen} onChange={handleChange('origen')} aria-invalid={Boolean(errors.origen)} aria-describedby={described('origen')}>{origenOptions.map((item) => <option key={item}>{item}</option>)}</select>{errors.origen ? <span id="origen-error" className="field-error">{errors.origen}</span> : null}</FormField>
        <FormField id="ejercicio" label="Ejercicio"><select ref={(el) => { if (el) refs.current.ejercicio = el; }} id="ejercicio" value={values.ejercicio} onChange={handleChange('ejercicio')} aria-invalid={Boolean(errors.ejercicio)} aria-describedby={described('ejercicio')}>{ejercicioOptions.map((item) => <option key={item}>{item}</option>)}</select>{errors.ejercicio ? <span id="ejercicio-error" className="field-error">{errors.ejercicio}</span> : null}</FormField>
        <FormField id="instrumento" label="Instrumento"><input ref={(el) => { if (el) refs.current.instrumento = el; }} id="instrumento" value={values.instrumento} onChange={handleChange('instrumento')} maxLength={30} aria-invalid={Boolean(errors.instrumento)} aria-describedby={described('instrumento')} />{errors.instrumento ? <span id="instrumento-error" className="field-error">{errors.instrumento}</span> : null}</FormField>
        <FormField id="fechaPago" label="Fecha de pago"><input ref={(el) => { if (el) refs.current.fechaPago = el; }} id="fechaPago" type="date" value={values.fechaPago} onChange={handleChange('fechaPago')} aria-invalid={Boolean(errors.fechaPago)} aria-describedby={described('fechaPago')} />{errors.fechaPago ? <span id="fechaPago-error" className="field-error">{errors.fechaPago}</span> : null}</FormField>
        <FormField id="secuenciaEvento" label="Secuencia de evento"><input ref={(el) => { if (el) refs.current.secuenciaEvento = el; }} id="secuenciaEvento" value={values.secuenciaEvento} onChange={handleChange('secuenciaEvento')} maxLength={20} aria-invalid={Boolean(errors.secuenciaEvento)} aria-describedby={described('secuenciaEvento')} />{errors.secuenciaEvento ? <span id="secuenciaEvento-error" className="field-error">{errors.secuenciaEvento}</span> : null}</FormField>
        <FormField id="factorActualizacion" label="Factor de actualización"><input ref={(el) => { if (el) refs.current.factorActualizacion = el; }} className="calculated-field" id="factorActualizacion" value={values.factorActualizacion} readOnly aria-readonly="true" aria-invalid={Boolean(errors.factorActualizacion)} aria-describedby={described('factorActualizacion', 'factor-help')} /><span id="factor-help" className="help-text">Cálculo referencial de demostración. Será reemplazado por las reglas tributarias del backend. No es un cálculo tributario real, no debe utilizarse en producción y no proviene de una API oficial.</span>{errors.factorActualizacion ? <span id="factorActualizacion-error" className="field-error">{errors.factorActualizacion}</span> : null}</FormField>
        <FormField id="monto" label="Monto"><input ref={(el) => { if (el) refs.current.monto = el; }} id="monto" type="number" min="0" step="0.01" value={values.monto} onChange={handleChange('monto')} aria-invalid={Boolean(errors.monto)} aria-describedby={described('monto')} />{errors.monto ? <span id="monto-error" className="field-error">{errors.monto}</span> : null}</FormField>
        <FormField id="estado" label="Estado"><select ref={(el) => { if (el) refs.current.estado = el; }} id="estado" value={values.estado} onChange={handleChange('estado')} aria-invalid={Boolean(errors.estado)} aria-describedby={described('estado')}>{estadoOptions.map((item) => <option key={item}>{item}</option>)}</select>{errors.estado ? <span id="estado-error" className="field-error">{errors.estado}</span> : null}</FormField>
        <FormField id="descripcion" label="Descripción"><textarea ref={(el) => { if (el) refs.current.descripcion = el; }} id="descripcion" value={values.descripcion} onChange={handleChange('descripcion')} maxLength={150} aria-invalid={Boolean(errors.descripcion)} aria-describedby={described('descripcion')} />{errors.descripcion ? <span id="descripcion-error" className="field-error">{errors.descripcion}</span> : null}</FormField>
      </div></fieldset>
      <div className="form-actions"><Button variant="primary" type="submit" disabled={isBusy}>{isBusy ? 'Procesando...' : submitLabel}</Button><Button type="button" onClick={reset} disabled={isBusy}>{resetLabel}</Button><Button type="button" onClick={cancel} disabled={isBusy}>Cancelar</Button></div>
    </form>
    <div className="form-result" aria-live="polite">{submitState === 'success' ? <><InlineMessage tone="success" message={successMessage} />{processed ? <div className="processed-summary"><h2>Resumen de valores procesados</h2><dl>{Object.entries(processed).map(([key, value]) => <div key={key}><dt>{fieldLabels[key as keyof ClassificationFormValues] ?? key}</dt><dd>{value}</dd></div>)}</dl></div> : null}</> : null}</div>
    {showDiscard ? <div className="discard-panel" role="dialog" aria-modal="false" aria-labelledby="discard-title"><h2 id="discard-title">Tiene cambios sin guardar.</h2><p>Puede continuar editando o descartar los cambios para volver al listado.</p><div className="filter-actions"><Button type="button" onClick={() => setShowDiscard(false)}>Continuar editando</Button><Button variant="primary" type="button" onClick={() => navigate('/calificaciones')}>Descartar cambios</Button></div></div> : null}
    <span className="metadata">Modo activo: {mode}</span>
  </section>;
}

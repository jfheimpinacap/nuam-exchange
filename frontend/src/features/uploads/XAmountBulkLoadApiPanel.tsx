import { useEffect, useRef, useState, type ChangeEvent } from 'react';
import { getUserFriendlyApiMessage } from '../../api/client/ApiError';
import type { BulkLoadXAmountResultDto, TaxClassificationsBulkLoadService } from '../../api/services/TaxClassificationsBulkLoadService';
import { Button } from '../../components/Button';
import { InlineMessage } from '../../components/InlineMessage';
import type { UserRole } from '../../types/session';

const REQUIRED_HEADER = 'market;instrumentCode;taxPeriod;referenceAmount';
type ApiStage = 'selection' | 'confirmation' | 'submitting' | 'completed';

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} bytes`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

function normalizeHeader(header: string): string {
  return header.replace(/^\uFEFF/, '').trim();
}

function firstLine(text: string): string {
  return text.replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n')[0] ?? '';
}

function isAuthorized(role: UserRole | undefined): boolean {
  return role === 'Administrador' || role === 'Analista Tributario';
}

function readFileAsUtf8(file: File): Promise<string> {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(String(reader.result ?? ''));
    reader.onerror = () => reject(new Error('No fue posible leer el archivo CSV.'));
    reader.readAsText(file, 'UTF-8');
  });
}

export function XAmountBulkLoadApiPanel({ role, service }: { role: UserRole | undefined; service: TaxClassificationsBulkLoadService | null }) {
  const [file, setFile] = useState<File | null>(null);
  const [stage, setStage] = useState<ApiStage>('selection');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [result, setResult] = useState<BulkLoadXAmountResultDto | null>(null);
  const controllerRef = useRef<AbortController | null>(null);
  const inputRef = useRef<HTMLInputElement | null>(null);

  useEffect(() => () => controllerRef.current?.abort(), []);

  const reset = () => {
    controllerRef.current?.abort();
    controllerRef.current = null;
    setFile(null);
    setStage('selection');
    setError('');
    setSuccess('');
    setResult(null);
    if (inputRef.current) inputRef.current.value = '';
  };

  const onFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    setFile(event.target.files?.[0] ?? null);
    setError('');
    setSuccess('');
    setResult(null);
    setStage('selection');
  };

  const validateAndContinue = async () => {
    setError('');
    setSuccess('');
    setResult(null);
    if (!file) {
      setError('Seleccione un archivo CSV antes de continuar.');
      return;
    }
    if (!file.name.toLowerCase().endsWith('.csv')) {
      setError('El archivo debe tener extensión .csv.');
      return;
    }
    try {
      const text = await readFileAsUtf8(file);
      const header = normalizeHeader(firstLine(text));
      if (header !== REQUIRED_HEADER) {
        setError(`El encabezado CSV debe ser exactamente: ${REQUIRED_HEADER}.`);
        return;
      }
      setStage('confirmation');
    } catch {
      setError('No fue posible leer el archivo como texto UTF-8.');
    }
  };

  const submit = async () => {
    if (!file || !service || stage === 'submitting') return;
    controllerRef.current?.abort();
    const controller = new AbortController();
    controllerRef.current = controller;
    setStage('submitting');
    setError('');
    setSuccess('');
    try {
      const response = await service.uploadXAmount(file, controller.signal);
      if (controller.signal.aborted) return;
      setResult(response);
      setSuccess('Carga masiva X Monto procesada correctamente.');
      setStage('completed');
    } catch (submissionError) {
      if (controller.signal.aborted) return;
      setError(submissionError instanceof Error && submissionError.message.startsWith('Respuesta inválida') ? submissionError.message : getUserFriendlyApiMessage(submissionError));
      setStage('confirmation');
    } finally {
      if (controllerRef.current === controller) controllerRef.current = null;
    }
  };

  if (!isAuthorized(role)) {
    return <InlineMessage tone="warning" message="No posee permisos para realizar cargas masivas. Solo Administrador y Analista Tributario pueden ejecutar la carga real de X Monto." />;
  }

  return <section className="bulk-load-api-panel" aria-live="polite">
    <InlineMessage tone="warning" message="Esta carga modificará datos reales de montos de referencia de calificaciones tributarias existentes." />
    {error ? <InlineMessage tone="error" message={error} /> : null}
    {success ? <InlineMessage tone="success" message={success} /> : null}
    {stage === 'selection' ? <div className="filters-panel x-amount-upload-controls">
      <label htmlFor="x-amount-file">Archivo CSV</label>
      <input id="x-amount-file" ref={inputRef} type="file" accept=".csv" onChange={onFileChange} />
      <p className="x-amount-required-header">Encabezado requerido: <code>{REQUIRED_HEADER}</code></p>
      {file ? <dl className="review-summary"><div><dt>Nombre</dt><dd>{file.name}</dd></div><div><dt>Tamaño</dt><dd>{formatBytes(file.size)}</dd></div></dl> : null}
      <Button type="button" variant="primary" onClick={validateAndContinue}>Continuar a confirmación</Button>
    </div> : null}
    {stage === 'confirmation' || stage === 'submitting' ? <div className="confirm-dialog inline-confirmation x-amount-confirmation">
      <h2>Archivo seleccionado</h2>
      <dl className="review-summary x-amount-confirmation-summary"><div><dt>Nombre</dt><dd>{file?.name}</dd></div><div><dt>Tamaño</dt><dd>{file ? formatBytes(file.size) : ''}</dd></div><div><dt>Encabezado validado</dt><dd>{REQUIRED_HEADER}</dd></div></dl>
      <InlineMessage tone="warning" message="Esta acción procesará el archivo y puede modificar montos de referencia de calificaciones tributarias existentes." />
      {stage === 'submitting' ? <div className="view-state"><div className="spinner" />Procesando carga...</div> : null}
      <div className="filter-actions"><Button type="button" variant="primary" onClick={submit} disabled={stage === 'submitting'}>Confirmar carga X Monto</Button><Button type="button" onClick={() => setStage('selection')} disabled={stage === 'submitting'}>Volver</Button><Button type="button" onClick={reset} disabled={stage === 'submitting'}>Cancelar</Button></div>
    </div> : null}
    {stage === 'completed' && result ? <div className="review-card">
      <h2>Resultado de la carga</h2>
      <dl className="review-summary"><div><dt>ID de carga</dt><dd>{result.uploadId}</dd></div><div><dt>Total de filas</dt><dd>{result.totalRows}</dd></div><div><dt>Filas exitosas</dt><dd>{result.successfulRows}</dd></div><div><dt>Filas con error</dt><dd>{result.failedRows}</dd></div><div><dt>Calificaciones actualizadas</dt><dd>{result.updatedTaxClassificationIds.length}</dd></div></dl>
      {result.errors.length ? <div className="table-scroll"><table className="data-table"><caption>Errores reportados por backend</caption><thead><tr><th>Fila</th><th>Código</th><th>Mensaje</th></tr></thead><tbody>{result.errors.map((item) => <tr key={`${item.rowNumber}-${item.code}-${item.message}`}><td>{item.rowNumber}</td><td>{item.code}</td><td>{item.message}</td></tr>)}</tbody></table></div> : null}
      <Button type="button" variant="primary" onClick={reset}>Iniciar una nueva carga</Button>
    </div> : null}
  </section>;
}

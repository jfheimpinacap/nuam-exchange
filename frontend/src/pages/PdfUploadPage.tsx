import { useEffect, useRef, useState, type ChangeEvent } from 'react';
import { useApiServices } from '../api/context/useApiServices';
import { getUserFriendlyApiMessage } from '../api/client/ApiError';
import type { PdfDocumentReviewResultDto } from '../api/services/DocumentReviewService';
import { useSession } from '../app/session/useSession';
import { Button } from '../components/Button';
import { InlineMessage } from '../components/InlineMessage';

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} bytes`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
const statusLabels: Record<string, string> = { VALID: 'Documento válido', INCOMPLETE: 'Documento incompleto', UNSUPPORTED: 'Documento no compatible', INVALID_FILE: 'Archivo inválido' };
const statusTone: Record<string, 'success' | 'warning' | 'error'> = { VALID: 'success', INCOMPLETE: 'warning', UNSUPPORTED: 'warning', INVALID_FILE: 'error' };

export function PdfUploadPage() {
  const { documentReviewService, isApi } = useApiServices();
  const { user } = useSession();
  const [file, setFile] = useState<File | null>(null);
  const [previewUrl, setPreviewUrl] = useState('');
  const [result, setResult] = useState<PdfDocumentReviewResultDto | null>(null);
  const [error, setError] = useState('');
  const [isProcessing, setIsProcessing] = useState(false);
  const controllerRef = useRef<AbortController | null>(null);
  const canProcessRole = user?.rol === 'Administrador' || user?.rol === 'Analista Tributario';

  useEffect(() => () => { controllerRef.current?.abort(); }, []);
  useEffect(() => {
    if (!file) { setPreviewUrl(''); return undefined; }
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);
    return () => URL.revokeObjectURL(url);
  }, [file]);

  const onFileChange = (event: ChangeEvent<HTMLInputElement>) => {
    const selected = event.target.files?.[0] ?? null;
    setFile(selected); setResult(null); setError('');
    if (selected && selected.type !== 'application/pdf' && !selected.name.toLowerCase().endsWith('.pdf')) setError('Seleccione un archivo con extensión PDF.');
  };

  const process = async () => {
    if (!file || !documentReviewService || isProcessing) return;
    if (!file.name.toLowerCase().endsWith('.pdf')) { setError('El archivo debe tener extensión .pdf.'); return; }
    controllerRef.current?.abort();
    const controller = new AbortController(); controllerRef.current = controller;
    setIsProcessing(true); setError(''); setResult(null);
    try {
      const response = await documentReviewService.reviewPdf(file, controller.signal);
      if (!controller.signal.aborted) setResult(response);
    } catch (submissionError) {
      if (!controller.signal.aborted) setError(getUserFriendlyApiMessage(submissionError));
    } finally {
      if (controllerRef.current === controller) controllerRef.current = null;
      setIsProcessing(false);
    }
  };

  return <div className="uploads-page pdf-upload-page">
    <section className="page-card">
      <h1>Carga PDF</h1>
      <p className="page-subtitle">Revisión documental tributaria ligera.</p>
      <InlineMessage message="Este módulo revisa PDFs con texto seleccionable. No realiza OCR sobre documentos escaneados." />
      {!isApi ? <InlineMessage tone="warning" message="La revisión PDF requiere conexión con la API." /> : null}
      {!canProcessRole ? <InlineMessage tone="warning" message="Solo Administrador y Analista Tributario pueden procesar PDFs. Supervisor no posee permiso de carga." /> : null}
      {error ? <InlineMessage tone="error" message={error} /> : null}
      <div className="filters-panel x-factor-upload-controls">
        <label htmlFor="pdf-file">Archivo PDF</label>
        <input id="pdf-file" type="file" accept="application/pdf,.pdf" onChange={onFileChange} disabled={isProcessing || !canProcessRole} />
        {file ? <dl className="review-summary"><div><dt>Nombre</dt><dd>{file.name}</dd></div><div><dt>Tamaño</dt><dd>{formatBytes(file.size)}</dd></div><div><dt>Validación</dt><dd>{file.name.toLowerCase().endsWith('.pdf') ? 'Archivo PDF seleccionado' : 'Extensión no válida'}</dd></div></dl> : null}
        <Button type="button" variant="primary" onClick={process} disabled={!file || isProcessing || !canProcessRole || !documentReviewService}>{isProcessing ? 'Procesando...' : 'Procesar PDF'}</Button>
      </div>
    </section>

    {result ? <section className="review-card">
      <h2>{statusLabels[result.status] ?? result.status}</h2>
      <InlineMessage tone={statusTone[result.status] ?? 'warning'} message={result.message} />
      <dl className="review-summary"><div><dt>Estado</dt><dd>{result.status}</dd></div><div><dt>Nombre</dt><dd>{result.fileName}</dd></div><div><dt>Tamaño</dt><dd>{formatBytes(result.fileSizeBytes)}</dd></div><div><dt>Páginas</dt><dd>{result.pageCount}</dd></div></dl>
      <div className="table-scroll"><table className="data-table"><caption>Campos detectados</caption><thead><tr><th>Campo</th><th>Valor</th></tr></thead><tbody>{Object.entries(result.detectedFields).map(([key, value]) => <tr key={key}><td>{key}</td><td>{value}</td></tr>)}</tbody></table></div>
      {result.missingFields.length ? <div className="error-summary"><strong>Campos faltantes</strong><ul>{result.missingFields.map((item) => <li key={item}>{item}</li>)}</ul></div> : null}
      {result.warnings.length ? <div className="error-summary"><strong>Advertencias</strong><ul>{result.warnings.map((item) => <li key={item}>{item}</li>)}</ul></div> : null}
      {result.textPreview ? <pre className="text-preview">{result.textPreview}</pre> : null}
    </section> : null}

    <section className="review-card">
      <h2>Vista previa del PDF</h2>
      {previewUrl ? <object data={previewUrl} type="application/pdf" width="100%" height="560"><p>No fue posible previsualizar el PDF en este navegador.</p></object> : <p>Seleccione un PDF para visualizarlo dentro del sistema.</p>}
    </section>
  </div>;
}

import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getUserFriendlyApiMessage } from '../api/client/ApiError';
import { useApiServices } from '../api/context/useApiServices';
import type { TaxClassificationDetailDto } from '../api/contracts/taxClassificationsRead';
import { Button } from '../components/Button';
import { LoadingState } from '../components/ViewStates';
import { TaxClassificationApiCopyView } from '../features/classifications/TaxClassificationApiCopyView';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { copyValuesFromClassification } from '../features/classifications/classificationFormUtils';
import { mockClassifications } from '../mocks/classifications';

export function ClassificationCopyPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { isApi, taxClassificationsReadService } = useApiServices();
  const [loading, setLoading] = useState(true);
  const [detail, setDetail] = useState<TaxClassificationDetailDto | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!isApi) { const timer = window.setTimeout(() => setLoading(false), 300); return () => window.clearTimeout(timer); }
    const numericId = Number(id);
    if (!id || !Number.isInteger(numericId) || numericId <= 0) { setLoading(false); setError('El identificador de la calificación tributaria no es válido.'); return undefined; }
    const controller = new AbortController(); setLoading(true); setError(''); setDetail(null);
    taxClassificationsReadService.getById(numericId, controller.signal).then(setDetail).catch((err: unknown) => { if (!controller.signal.aborted) setError(getUserFriendlyApiMessage(err)); }).finally(() => { if (!controller.signal.aborted) setLoading(false); });
    return () => controller.abort();
  }, [id, isApi, taxClassificationsReadService]);

  const record = useMemo(() => mockClassifications.find((item) => item.id === id), [id]);
  if (loading) return <LoadingState />;
  if (isApi) {
    const numericId = Number(id);
    if (error) return <section className="content-card not-found-panel"><h1>No fue posible cargar la calificación</h1><p>{error}</p><Button variant="primary" onClick={() => navigate('/calificaciones')}>Volver al listado</Button></section>;
    if (!detail || !Number.isInteger(numericId) || numericId <= 0) return <section className="content-card not-found-panel"><h1>Registro no encontrado</h1><p>No existe una calificación tributaria con el identificador solicitado.</p><Button variant="primary" onClick={() => navigate('/calificaciones')}>Volver al listado</Button></section>;
    return <TaxClassificationApiCopyView id={numericId} detail={detail} />;
  }
  if (!record) return <section className="content-card not-found-panel"><h1>Registro no encontrado</h1><p>No existe una calificación tributaria con el identificador solicitado.</p><Button variant="primary" onClick={() => navigate('/calificaciones')}>Volver al listado</Button></section>;
  return <ClassificationForm mode="copy" title="Copiar Calificación Tributaria" initialValues={copyValuesFromClassification(record)} infoMessage="Revise los datos y complete una nueva secuencia de evento antes de crear la copia." submitLabel="Crear copia" resetLabel="Restablecer" successMessage="Copia validada correctamente. Esta demostración no crea un nuevo registro." />;
}

import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '../components/Button';
import { LoadingState } from '../components/ViewStates';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { copyValuesFromClassification } from '../features/classifications/classificationFormUtils';
import { mockClassifications } from '../mocks/classifications';

export function ClassificationCopyPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  useEffect(() => { const timer = window.setTimeout(() => setLoading(false), 300); return () => window.clearTimeout(timer); }, [id]);
  const record = useMemo(() => mockClassifications.find((item) => item.id === id), [id]);
  if (loading) return <LoadingState />;
  if (!record) return <section className="content-card not-found-panel"><h1>Registro no encontrado</h1><p>No existe una calificación tributaria con el identificador solicitado.</p><Button variant="primary" onClick={() => navigate('/calificaciones')}>Volver al listado</Button></section>;
  return <ClassificationForm mode="copy" title="Copiar Calificación Tributaria" initialValues={copyValuesFromClassification(record)} infoMessage="Revise los datos y complete una nueva secuencia de evento antes de crear la copia." submitLabel="Crear copia" resetLabel="Restablecer" successMessage="Copia validada correctamente. Esta demostración no crea un nuevo registro." />;
}

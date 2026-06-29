import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Button } from '../components/Button';
import { LoadingState } from '../components/ViewStates';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { valuesFromClassification } from '../features/classifications/classificationFormUtils';
import { mockClassifications } from '../mocks/classifications';

export function ClassificationEditPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  useEffect(() => { const timer = window.setTimeout(() => setLoading(false), 300); return () => window.clearTimeout(timer); }, [id]);
  const record = useMemo(() => mockClassifications.find((item) => item.id === id), [id]);
  if (loading) return <LoadingState />;
  if (!record) return <section className="content-card not-found-panel"><h1>Registro no encontrado</h1><p>No existe una calificación tributaria con el identificador solicitado.</p><Button variant="primary" onClick={() => navigate('/calificaciones')}>Volver al listado</Button></section>;
  return <ClassificationForm mode="edit" title="Modificar Calificación Tributaria" initialValues={valuesFromClassification(record)} submitLabel="Guardar cambios" resetLabel="Restablecer" successMessage="Cambios validados correctamente. Esta demostración no modifica el registro original." />;
}

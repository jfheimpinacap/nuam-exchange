import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { createInitialValues } from '../features/classifications/classificationFormConfig';

export function ClassificationCreatePage() {
  return <ClassificationForm mode="create" title="Ingresar Calificación Tributaria" initialValues={createInitialValues} submitLabel="Guardar" resetLabel="Limpiar" successMessage="Calificación validada correctamente. Esta demostración no guarda información." />;
}

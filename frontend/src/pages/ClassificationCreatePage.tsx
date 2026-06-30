import { useApiServices } from '../api/context/useApiServices';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { createInitialValues } from '../features/classifications/classificationFormConfig';
import { TaxClassificationApiForm } from '../features/classifications/TaxClassificationApiForm';
import { emptyTaxClassificationApiValues } from '../features/classifications/taxClassificationApiFormValues';

export function ClassificationCreatePage() {
  const { isApi } = useApiServices();
  if (isApi) return <TaxClassificationApiForm mode="create" initialValues={emptyTaxClassificationApiValues()} />;
  return <ClassificationForm mode="create" title="Ingresar Calificación Tributaria" initialValues={createInitialValues} submitLabel="Guardar" resetLabel="Limpiar" successMessage="Calificación validada correctamente. Esta demostración no guarda información." />;
}

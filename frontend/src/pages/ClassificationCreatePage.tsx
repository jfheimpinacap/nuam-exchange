import { PageHeader } from '../components/PageHeader';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
export function ClassificationCreatePage() { return <section className="page"><PageHeader title="Nueva calificación" description="Ingreso visual con validaciones locales." /><ClassificationForm mode="create" /></section>; }

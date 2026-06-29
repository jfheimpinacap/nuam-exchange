import { useLocation } from 'react-router-dom';
import { PageHeader } from '../components/PageHeader';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { mockClassifications } from '../mocks/classifications';
export function ClassificationCopyPage() { const id=useLocation().pathname.split('/')[2]; const initial=mockClassifications.find(item=>item.id===id); return <section className="page"><PageHeader title="Copiar calificación" description="Copia visual basada en un registro existente." /><ClassificationForm mode="copy" initial={initial} /></section>; }

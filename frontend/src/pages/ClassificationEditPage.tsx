import { useLocation } from 'react-router-dom';
import { PageHeader } from '../components/PageHeader';
import { ClassificationForm } from '../features/classifications/ClassificationForm';
import { mockClassifications } from '../mocks/classifications';
export function ClassificationEditPage() { const id=useLocation().pathname.split('/')[2]; const initial=mockClassifications.find(item=>item.id===id); return <section className="page"><PageHeader title="Editar calificación" description="Edición demostrativa sin persistencia." /><ClassificationForm mode="edit" initial={initial} /></section>; }

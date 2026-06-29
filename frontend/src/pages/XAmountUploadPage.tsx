import { PageHeader } from '../components/PageHeader';
import { UploadWorkspace } from '../features/uploads/UploadWorkspace';
export function XAmountUploadPage() { return <section className="page"><PageHeader title="Carga masiva X Monto" description="Procesamiento CSV simulado con validación local." /><UploadWorkspace kind="X Monto" /></section>; }

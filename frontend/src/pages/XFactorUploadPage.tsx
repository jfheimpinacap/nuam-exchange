import { PageHeader } from '../components/PageHeader';
import { UploadWorkspace } from '../features/uploads/UploadWorkspace';
export function XFactorUploadPage() { return <section className="page"><PageHeader title="Carga masiva X Factor" description="Procesamiento CSV simulado con vista previa local." /><UploadWorkspace kind="X Factor" /></section>; }

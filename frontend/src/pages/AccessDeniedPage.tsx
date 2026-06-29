import { useNavigate } from 'react-router-dom';
import { PageHeader } from '../components/PageHeader';
import { useSession } from '../app/session/useSession';

export function AccessDeniedPage() {
  const navigate = useNavigate();
  const { user } = useSession();

  return (
    <section className="content-card access-denied" aria-labelledby="access-denied-title">
      <PageHeader
        title="Acceso restringido"
        description={`El rol ${user?.rol ?? 'actual'} no tiene permisos para acceder a esta sección.`}
      />
      <p>Solicita la habilitación correspondiente si necesitas operar este módulo.</p>
      <button type="button" className="primary-button" onClick={() => navigate('/inicio')}>
        Volver al inicio
      </button>
    </section>
  );
}

import { mockClassifications } from '../mocks/classifications';
import { StatCard } from '../components/StatCard';
import { money } from '../utils/formatters';

export function DashboardPage() {
  const total = mockClassifications.reduce((sum, item) => sum + item.amount, 0);
  return <section className="page"><h1>Inicio</h1><p>Panel administrativo simulado para preparar la futura integración con ASP.NET Core.</p><div className="stats"><StatCard label="Calificaciones" value={String(mockClassifications.length)} hint="Registros mock" /><StatCard label="Monto total" value={money.format(total)} hint="Suma en memoria" /><StatCard label="Mercados" value="3" hint="Chile, Colombia y Perú" /></div></section>;
}

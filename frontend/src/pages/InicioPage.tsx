import { useMemo, useState } from 'react';
import { mockClassifications } from '../mocks/classifications';
import { uploadReviews } from '../mocks/uploadReview';
import { dashboardActivity } from '../mocks/dashboardActivity';
import { useSession } from '../app/session/useSession';
import { DashboardFilters } from '../features/dashboard/DashboardFilters';
import { MetricCard } from '../features/dashboard/MetricCard';
import { MarketAmountChart } from '../features/dashboard/MarketAmountChart';
import { QuickActions } from '../features/dashboard/QuickActions';
import { RecentActivityTable } from '../features/dashboard/RecentActivityTable';
import { StatusDistribution } from '../features/dashboard/StatusDistribution';
import { buildMetrics, filterClassifications, marketAmounts, money, recentActivity, statusItems } from '../features/dashboard/dashboardUtils';
import type { DashboardFilters as Filters } from '../types/dashboard';
const initialFilters: Filters = { year: 'Todos', market: 'Todos' };
export function InicioPage() {
  const { user } = useSession();
  const [draft, setDraft] = useState<Filters>(initialFilters);
  const [active, setActive] = useState<Filters>(initialFilters);
  const records = useMemo(()=>filterClassifications(mockClassifications, active), [active]);
  const metrics = useMemo(()=>buildMetrics(records, uploadReviews), [records]);
  if (!user) return null;
  return <div className="dashboard-page"><><DashboardFilters draft={draft} active={active} onChange={setDraft} onApply={()=>setActive(draft)} onClear={()=>{setDraft(initialFilters); setActive(initialFilters);}} />
      <section className="metrics-grid" aria-live="polite"><MetricCard label="Total de Calificaciones" value={metrics.total} description="Registros tributarios filtrados." /><MetricCard label="Vigentes" value={metrics.vigente} description="Calificaciones listas para consulta." /><MetricCard label="Pendientes" value={metrics.pendiente} description="Registros que requieren gestión." /><MetricCard label="Observadas" value={metrics.observada} description="Registros con observaciones." /><MetricCard label="Monto total" value={money.format(metrics.montoTotal)} description="Suma de montos filtrados." /><MetricCard label="Cargas registradas" value={metrics.cargas} description="Cargas masivas simuladas disponibles." /><MetricCard label="Filas con errores" value={metrics.filasConError} description="Filas inválidas detectadas en cargas." /></section>
      <div className="charts-grid"><StatusDistribution items={statusItems(records)} /><MarketAmountChart items={marketAmounts(records)} /></div><QuickActions role={user.rol} /><RecentActivityTable rows={recentActivity(mockClassifications, uploadReviews, dashboardActivity)} /></>
  </div>;
}

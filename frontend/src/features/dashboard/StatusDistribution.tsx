import { AccessibleBarChart } from './AccessibleBarChart';
import type { ChartItem } from '../../types/dashboard';
export function StatusDistribution({ items }: { items: ChartItem[] }) { return <AccessibleBarChart title="Distribución por estado" description="Distribución accesible de calificaciones por estado, con valores absolutos y porcentajes." items={items} />; }

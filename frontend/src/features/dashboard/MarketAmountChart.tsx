import { AccessibleBarChart } from './AccessibleBarChart';
import type { ChartItem } from '../../types/dashboard';
export function MarketAmountChart({ items }: { items: ChartItem[] }) { return <AccessibleBarChart title="Monto por mercado" description="Monto acumulado por mercado respetando los filtros aplicados." items={items} />; }

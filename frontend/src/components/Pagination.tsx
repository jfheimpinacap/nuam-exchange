import { Button } from './Button';
import type { PaginationState } from '../types/classification';

interface PaginationProps { pagination: PaginationState; totalItems: number; onPageChange: (page: number) => void; onPageSizeChange: (pageSize: PaginationState['pageSize']) => void; }
export function Pagination({ pagination, totalItems, onPageChange, onPageSizeChange }: PaginationProps) {
  const totalPages = Math.max(1, Math.ceil(totalItems / pagination.pageSize));
  const start = totalItems === 0 ? 0 : (pagination.page - 1) * pagination.pageSize + 1;
  const end = Math.min(totalItems, pagination.page * pagination.pageSize);
  return <div className="pagination" aria-label="Paginación de calificaciones">
    <span>Mostrando {start}–{end} de {totalItems} registros.</span>
    <div className="pagination-controls">
      <Button onClick={() => onPageChange(pagination.page - 1)} disabled={pagination.page <= 1}>Anterior</Button>
      <span>Página {pagination.page} de {totalPages}</span>
      <Button onClick={() => onPageChange(pagination.page + 1)} disabled={pagination.page >= totalPages}>Siguiente</Button>
      <label htmlFor="page-size">Registros por página</label>
      <select id="page-size" value={pagination.pageSize} onChange={(event) => onPageSizeChange(Number(event.target.value) as PaginationState['pageSize'])}>
        <option value={5}>5</option><option value={10}>10</option><option value={20}>20</option><option value={50}>50</option>
      </select>
    </div>
  </div>;
}

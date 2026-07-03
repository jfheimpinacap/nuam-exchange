import { useMemo, useState } from 'react';
import { EmptyState } from '../components/ViewStates';
import { Pagination } from '../components/Pagination';
import { auditEvents } from '../mocks/auditEvents';
import type { AuditEvent, AuditSortKey, AuditSortState } from '../types/audit';
import { AuditDetailDialog } from '../features/audit/AuditDetailDialog';
import { AuditFilters } from '../features/audit/AuditFilters';
import { AuditSummary } from '../features/audit/AuditSummary';
import { AuditTable } from '../features/audit/AuditTable';
import { exportAudit } from '../features/audit/auditExport';
import { filterAudit, initialAuditFilters, sortAudit, validRange } from '../features/audit/auditUtils';

export function AuditPage() {
  const [draft, setDraft] = useState(initialAuditFilters);
  const [active, setActive] = useState(initialAuditFilters);
  const [error, setError] = useState('');
  const [sort, setSort] = useState<AuditSortState>({ key: 'fechaHora', direction: 'desc' });
  const [page, setPage] = useState({ page: 1, pageSize: 10 as 10 | 20 | 50 });
  const [detail, setDetail] = useState<AuditEvent | null>(null);
  const filtered = useMemo(() => sortAudit(filterAudit(auditEvents, active), sort), [active, sort]);
  const pageRows = filtered.slice((page.page - 1) * page.pageSize, page.page * page.pageSize);
  const users = Array.from(new Set(auditEvents.map((event) => event.usuarioNombre)));
  const search = () => {
    if (!validRange(draft.desde, draft.hasta)) {
      setError('La fecha desde no puede ser posterior a la fecha hasta.');
      return;
    }
    setError('');
    setActive(draft);
    setPage({ ...page, page: 1 });
  };
  const clear = () => {
    setDraft(initialAuditFilters);
    setActive(initialAuditFilters);
    setError('');
    setPage({ ...page, page: 1 });
  };
  const onSort = (key: AuditSortKey) => setSort({ key, direction: sort.key === key && sort.direction === 'asc' ? 'desc' : 'asc' });

  return (
    <section className="admin-page">
      <AuditSummary events={filtered} />
      <AuditFilters draft={draft} users={users} error={error} onChange={setDraft} onSearch={search} onClear={clear} onExport={() => exportAudit(filtered)} />
      {filtered.length === 0 ? (
        <EmptyState title="Sin eventos" description="No hay eventos para los filtros aplicados." actionLabel="Limpiar filtros" onAction={clear} />
      ) : (
        <>
          <AuditTable events={pageRows} sort={sort} onSort={onSort} onDetail={setDetail} />
          <Pagination pagination={page} totalItems={filtered.length} onPageChange={(nextPage) => setPage({ ...page, page: nextPage })} onPageSizeChange={(pageSize) => setPage({ page: 1, pageSize: pageSize as 10 | 20 | 50 })} />
        </>
      )}
      {detail ? <AuditDetailDialog event={detail} onClose={() => setDetail(null)} /> : null}
    </section>
  );
}

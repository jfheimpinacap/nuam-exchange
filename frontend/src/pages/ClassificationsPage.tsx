import { useEffect, useMemo, useState, type FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserFriendlyApiMessage } from '../api/client/ApiError';
import { useClassificationsQuery } from '../api/hooks/useClassificationsQuery';
import { useSession } from '../app/session/useSession';
import { Button } from '../components/Button';
import { DataTableShell } from '../components/DataTableShell';
import { EmptyState, ErrorState, LoadingState } from '../components/ViewStates';
import { FormField } from '../components/FormField';
import { InlineMessage } from '../components/InlineMessage';
import { PageHeader } from '../components/PageHeader';
import { Pagination } from '../components/Pagination';
import type { Classification, ClassificationFilters, ClassificationSortKey, PaginationState, SortState } from '../types/classification';
import type { ClassificationListRequestDto, ClassificationSortByDto } from '../api/contracts/classifications';

const emptyFilters: ClassificationFilters = { mercado: 'Todos', origen: 'Todos', ejercicio: 'Todos', estado: 'Todos', texto: '' };
type DemoState = 'Normal' | 'Cargando' | 'Error';
const sortMap: Record<ClassificationSortKey, ClassificationSortByDto> = { ejercicio: 'fiscalYear', instrumento: 'instrument', fechaPago: 'paymentDate', monto: 'amount', estado: 'status' };
function optionalFilter(value: string) { return value === 'Todos' ? undefined : value; }

export function ClassificationsPage() {
  const navigate = useNavigate();
  const { user } = useSession();
  const [draftFilters, setDraftFilters] = useState<ClassificationFilters>(emptyFilters);
  const [appliedFilters, setAppliedFilters] = useState<ClassificationFilters>(emptyFilters);
  const [sort, setSort] = useState<SortState>({ key: 'ejercicio', direction: 'asc' });
  const [pagination, setPagination] = useState<PaginationState>({ page: 1, pageSize: 10 });
  const [selected, setSelected] = useState<Classification | null>(null);
  const [message, setMessage] = useState('Seleccione un registro para habilitar acciones contextuales.');
  const [demoState, setDemoState] = useState<DemoState>('Normal');
  const request = useMemo<ClassificationListRequestDto>(() => ({ page: pagination.page, pageSize: pagination.pageSize === 50 ? 20 : pagination.pageSize, search: appliedFilters.texto.trim() || undefined, market: optionalFilter(appliedFilters.mercado), source: optionalFilter(appliedFilters.origen), fiscalYear: appliedFilters.ejercicio === 'Todos' ? undefined : Number(appliedFilters.ejercicio), status: optionalFilter(appliedFilters.estado) as ClassificationListRequestDto['status'], sortBy: sortMap[sort.key], sortDirection: sort.direction }), [appliedFilters, pagination.page, pagination.pageSize, sort]);
  const { data, catalogs, isLoading, error, reload } = useClassificationsQuery(request);

  useEffect(() => { if (pagination.page > Math.max(1, data.totalPages)) setPagination((current) => ({ ...current, page: Math.max(1, data.totalPages) })); }, [data.totalPages, pagination.page]);
  useEffect(() => { if (selected && !data.items.some((record) => record.id === selected.id)) setSelected(null); }, [data.items, selected]);

  const selectOptions = (values: Array<string | number>) => ['Todos', ...values.map(String)];
  const canEdit = user?.rol === 'Administrador' || user?.rol === 'Analista Tributario';
  const canEnter = canEdit;
  const canMassLoad = Boolean(user);
  const from = data.totalItems === 0 ? 0 : (data.page - 1) * data.pageSize + 1;
  const to = Math.min(data.page * data.pageSize, data.totalItems);

  function applyFilters(event: FormEvent) { event.preventDefault(); setAppliedFilters(draftFilters); setPagination((current) => ({ ...current, page: 1 })); setSelected(null); setMessage('Filtros aplicados mediante servicio de datos.'); }
  function clearFilters() { setDraftFilters(emptyFilters); setAppliedFilters(emptyFilters); setPagination((current) => ({ ...current, page: 1 })); setSelected(null); setMessage('Filtros limpiados.'); }
  function handleSort(key: ClassificationSortKey) { setSort((current) => ({ key, direction: current.key === key && current.direction === 'asc' ? 'desc' : 'asc' })); setPagination((current) => ({ ...current, page: 1 })); }
  function requireSelection(action: string, callback: (record: Classification) => void) { if (!selected) { setMessage(`${action} requiere seleccionar un registro.`); return; } callback(selected); }

  return <section className="content-card classifications-page">
    <PageHeader title="Calificaciones Tributarias" description="Consulta y administración visual de registros tributarios." />
    <div className="demo-panel"><FormField id="demo-state" label="Estado de demostración"><select id="demo-state" value={demoState} onChange={(event) => setDemoState(event.target.value as DemoState)}><option>Normal</option><option>Cargando</option><option>Error</option></select></FormField><span>Control temporal para simular estados visuales sin cambiar la fuente de datos.</span></div>
    <form className="filters-panel" onSubmit={applyFilters}>
      <FormField id="mercado" label="Mercado"><select id="mercado" value={draftFilters.mercado} onChange={(e) => setDraftFilters({ ...draftFilters, mercado: e.target.value })}>{selectOptions(catalogs.markets).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="origen" label="Origen"><select id="origen" value={draftFilters.origen} onChange={(e) => setDraftFilters({ ...draftFilters, origen: e.target.value })}>{selectOptions(catalogs.sources).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="ejercicio" label="Ejercicio"><select id="ejercicio" value={draftFilters.ejercicio} onChange={(e) => setDraftFilters({ ...draftFilters, ejercicio: e.target.value })}>{selectOptions(catalogs.fiscalYears).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="estado" label="Estado"><select id="estado" value={draftFilters.estado} onChange={(e) => setDraftFilters({ ...draftFilters, estado: e.target.value })}>{selectOptions(catalogs.statuses).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="texto" label="Texto libre"><input id="texto" value={draftFilters.texto} onChange={(e) => setDraftFilters({ ...draftFilters, texto: e.target.value })} placeholder="Instrumento, descripción o secuencia" /></FormField>
      <div className="filter-actions"><Button variant="primary" type="submit">Buscar</Button><Button type="button" onClick={clearFilters}>Limpiar</Button></div>
    </form>
    <div className="actions-bar" aria-label="Acciones principales">
      {canEnter ? <Button variant="primary" onClick={() => navigate('/calificaciones/nueva')}>Ingresar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => requireSelection('Modificar', (record) => navigate(`/calificaciones/${record.id}/editar`))}>Modificar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => requireSelection('Eliminar', () => setMessage('La eliminación real se implementará posteriormente.'))}>Eliminar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => requireSelection('Copiar', (record) => navigate(`/calificaciones/${record.id}/copiar`))}>Copiar</Button> : null}
      {canMassLoad ? <Button onClick={() => navigate('/cargas/x-factor')}>Carga X Factor</Button> : null}
      {canMassLoad ? <Button onClick={() => navigate('/cargas/x-monto')}>Carga X Monto</Button> : null}
      <Button disabled={!selected} onClick={() => requireSelection('Opciones', (record) => setMessage(`Registro ${record.id}: ${record.instrumento}, ${record.estado}, monto ${record.monto.toLocaleString('es-CL')}.`))}>Opciones</Button>
    </div>
    <InlineMessage message={`${message} Mostrando ${from}–${to} de ${data.totalItems} registros.`} />
    {demoState === 'Error' ? <ErrorState title="Error de demostración" description="Estado temporal para validar la vista de error antes de integrar la API." /> : demoState === 'Cargando' || isLoading ? <LoadingState /> : error ? <div className="view-state view-state-error" role="alert"><strong>No fue posible cargar calificaciones</strong><span>{getUserFriendlyApiMessage(error)}</span><Button onClick={reload}>Reintentar</Button></div> : data.totalItems === 0 ? <EmptyState title="Sin resultados" description="No existen calificaciones para los filtros aplicados." actionLabel="Limpiar filtros" onAction={clearFilters} /> : <><DataTableShell records={data.items} selectedId={selected?.id ?? null} sort={sort} onSort={handleSort} onSelect={(record) => { setSelected(record); setMessage(`Registro seleccionado: ${record.id}.`); }} /><Pagination pagination={pagination} totalItems={data.totalItems} onPageChange={(page) => setPagination((current) => ({ ...current, page }))} onPageSizeChange={(pageSize) => setPagination({ page: 1, pageSize })} /></>}
  </section>;
}

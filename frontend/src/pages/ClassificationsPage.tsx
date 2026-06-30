import { useEffect, useMemo, useRef, useState, type FormEvent, type ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserFriendlyApiMessage, isApiError, ApiError } from '../api/client/ApiError';
import type { TaxClassificationDetailDto, TaxClassificationListRequestDto, TaxClassificationReadDto, TaxClassificationSortByDto, TaxClassificationSortState, TaxClassificationUiSortKey } from '../api/contracts/taxClassificationsRead';
import { useTaxClassificationsReadQuery } from '../api/hooks/useTaxClassificationsReadQuery';
import { useApiServices } from '../api/context/useApiServices';
import { useSession } from '../app/session/useSession';
import { Button } from '../components/Button';
import { DataTableShell } from '../components/DataTableShell';
import { EmptyState, ErrorState, LoadingState } from '../components/ViewStates';
import { FormField } from '../components/FormField';
import { InlineMessage } from '../components/InlineMessage';
import { PageHeader } from '../components/PageHeader';
import { Pagination } from '../components/Pagination';
import { formatIsoDateForDisplay } from '../utils/dateFormatting';
import type { PaginationState } from '../types/classification';

interface TaxClassificationReadFilters { mercado: string; ejercicio: string; estado: string; texto: string; }
const emptyFilters: TaxClassificationReadFilters = { mercado: 'Todos', ejercicio: 'Todos', estado: 'Todos', texto: '' };
type DemoState = 'Normal' | 'Cargando' | 'Error';
const sortMap: Record<TaxClassificationUiSortKey, TaxClassificationSortByDto> = { taxPeriod: 'taxPeriod', instrumentCode: 'instrumentCode', validFrom: 'validFrom', status: 'status', market: 'market' };
const dash = '—';
function optionalFilter(value: string) { return value === 'Todos' ? undefined : value; }
function selectOptions(values: Array<string | number>) { return ['Todos', ...values.map(String)]; }
function statusLabel(status: string) { return status.toLowerCase().replace(/(^|\s|_|-)(\p{L})/gu, (_match, separator: string, letter: string) => `${separator === '_' || separator === '-' ? ' ' : separator}${letter.toUpperCase()}`); }
function display(value: string | null | undefined) { return value?.trim() || dash; }
function formatDate(value: string | null | undefined) { return value?.trim() ? formatIsoDateForDisplay(value, { includeTime: value.includes('T') }) : dash; }
function formatNumber(value: number | null | undefined, maximumFractionDigits = 6) { return value === null || value === undefined ? dash : value.toLocaleString('es-CL', { maximumFractionDigits }); }
function formatMoney(value: number | null | undefined, currency: string | null | undefined) { return value === null || value === undefined ? dash : new Intl.NumberFormat('es-CL', { style: 'currency', currency: currency || 'CLP', maximumFractionDigits: currency === 'CLP' ? 0 : 2 }).format(value); }

type DetailField = { label: string; value: ReactNode };
function getDetailFields(detail: TaxClassificationDetailDto): DetailField[] {
  return [
    { label: 'Mercado', value: detail.market },
    { label: 'Código y nombre del instrumento', value: `${display(detail.instrumentCode)} / ${display(detail.instrumentName)}` },
    { label: 'Tipo de clasificación', value: detail.classificationType },
    { label: 'Descripción', value: display(detail.description) },
    { label: 'Porcentaje de actualización', value: formatNumber(detail.updatePercentage) },
    { label: 'Factor aplicado', value: formatNumber(detail.appliedFactor) },
    { label: 'Monto de referencia y moneda', value: formatMoney(detail.referenceAmount, detail.currency) },
    { label: 'Período tributario', value: detail.taxPeriod },
    { label: 'Vigencia desde', value: formatDate(detail.validFrom) },
    { label: 'Vigencia hasta', value: formatDate(detail.validTo) },
    { label: 'Estado', value: statusLabel(detail.status) },
    { label: 'Creado en', value: formatDate(detail.createdAt) },
    { label: 'Actualizado en', value: formatDate(detail.updatedAt) },
  ];
}

export function ClassificationsPage() {
  const navigate = useNavigate();
  const { user } = useSession();
  const { isApi, taxClassificationsReadService } = useApiServices();
  const [draftFilters, setDraftFilters] = useState<TaxClassificationReadFilters>(emptyFilters);
  const [appliedFilters, setAppliedFilters] = useState<TaxClassificationReadFilters>(emptyFilters);
  const [sort, setSort] = useState<TaxClassificationSortState>({ key: 'taxPeriod', direction: 'asc' });
  const [pagination, setPagination] = useState<PaginationState>({ page: 1, pageSize: 10 });
  const [selected, setSelected] = useState<TaxClassificationReadDto | null>(null);
  const [message, setMessage] = useState('Seleccione un registro para consultar su detalle.');
  const [demoState, setDemoState] = useState<DemoState>('Normal');
  const [detail, setDetail] = useState<TaxClassificationDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState<ApiError | null>(null);
  const detailRequestRef = useRef(0);
  const detailControllerRef = useRef<AbortController | null>(null);
  const request = useMemo<TaxClassificationListRequestDto>(() => ({ page: pagination.page, pageSize: pagination.pageSize, search: appliedFilters.texto.trim() || undefined, market: optionalFilter(appliedFilters.mercado), exercise: appliedFilters.ejercicio === 'Todos' ? undefined : Number(appliedFilters.ejercicio), status: optionalFilter(appliedFilters.estado), sortBy: sortMap[sort.key], sortDirection: sort.direction }), [appliedFilters, pagination.page, pagination.pageSize, sort]);
  const { data, filterOptions, isLoading, error, reload } = useTaxClassificationsReadQuery(request);

  useEffect(() => { if (pagination.page > Math.max(1, data.totalPages)) setPagination((current) => ({ ...current, page: Math.max(1, data.totalPages) })); }, [data.totalPages, pagination.page]);
  useEffect(() => { if (selected && !data.items.some((record) => record.id === selected.id)) { setSelected(null); setDetail(null); } }, [data.items, selected]);

  const canEdit = !isApi && (user?.rol === 'Administrador' || user?.rol === 'Analista Tributario');
  const canEnter = canEdit;
  const canMassLoad = !isApi && Boolean(user);
  const from = data.totalCount === 0 ? 0 : (data.page - 1) * data.pageSize + 1;
  const to = Math.min(data.page * data.pageSize, data.totalCount);

  function loadDetail(record: TaxClassificationReadDto) {
    setSelected(record); setMessage(`Registro seleccionado: ${record.id}. Consultando detalle real.`); setDetail(null); setDetailError(null); setDetailLoading(true);
    detailControllerRef.current?.abort(); const controller = new AbortController(); detailControllerRef.current = controller; const requestId = detailRequestRef.current + 1; detailRequestRef.current = requestId;
    taxClassificationsReadService.getById(record.id, controller.signal).then((value) => { if (!controller.signal.aborted && detailRequestRef.current === requestId) setDetail(value); }).catch((err: unknown) => { if (!controller.signal.aborted && detailRequestRef.current === requestId) setDetailError(isApiError(err) ? err : new ApiError({ code: 'INVALID_RESPONSE', message: 'Respuesta inválida.', cause: err })); }).finally(() => { if (!controller.signal.aborted && detailRequestRef.current === requestId) setDetailLoading(false); });
  }
  function applyFilters(event: FormEvent) { event.preventDefault(); setAppliedFilters(draftFilters); setPagination((current) => ({ ...current, page: 1 })); setSelected(null); setDetail(null); setMessage('Filtros aplicados mediante servicio de lectura.'); }
  function clearFilters() { setDraftFilters(emptyFilters); setAppliedFilters(emptyFilters); setPagination((current) => ({ ...current, page: 1 })); setSelected(null); setDetail(null); setMessage('Filtros limpiados.'); }
  function handleSort(key: TaxClassificationUiSortKey) { setSort((current) => ({ key, direction: current.key === key && current.direction === 'asc' ? 'desc' : 'asc' })); setPagination((current) => ({ ...current, page: 1 })); }
  async function navigateToMockWrite(pathSegment: 'editar' | 'copiar') {
    if (!selected) {
      setMessage('Seleccione un registro antes de continuar.');
      return;
    }

    const { mockClassifications } = await import('../mocks/classifications');
    const mockRecord = mockClassifications[selected.id - 1];
    if (!mockRecord) {
      setMessage('No fue posible resolver el identificador histórico del registro mock seleccionado.');
      return;
    }

    navigate(`/calificaciones/${mockRecord.id}/${pathSegment}`);
  }
  function closeDetail() { detailControllerRef.current?.abort(); setDetail(null); setDetailError(null); setDetailLoading(false); setSelected(null); setMessage('Detalle cerrado.'); }

  return <section className="content-card classifications-page">
    <PageHeader title="Calificaciones Tributarias" description="Consulta de registros tributarios y detalle desde el servicio configurado." />
    {isApi ? <InlineMessage message="Esta etapa integra solo consulta y detalle. Ingresar, modificar, eliminar, copiar y cargas masivas se integrarán posteriormente." /> : null}
    {!isApi ? <div className="demo-panel"><FormField id="demo-state" label="Estado de demostración"><select id="demo-state" value={demoState} onChange={(event) => setDemoState(event.target.value as DemoState)}><option>Normal</option><option>Cargando</option><option>Error</option></select></FormField><span>Control temporal para simular estados visuales sin cambiar la fuente de datos.</span></div> : null}
    <form className="filters-panel" onSubmit={applyFilters}>
      <FormField id="mercado" label="Mercado"><select id="mercado" value={draftFilters.mercado} onChange={(e) => setDraftFilters({ ...draftFilters, mercado: e.target.value })}>{selectOptions(filterOptions.markets).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="ejercicio" label="Período tributario"><select id="ejercicio" value={draftFilters.ejercicio} onChange={(e) => setDraftFilters({ ...draftFilters, ejercicio: e.target.value })}>{selectOptions(filterOptions.exercises).map((item) => <option key={item}>{item}</option>)}</select></FormField>
      <FormField id="estado" label="Estado"><select id="estado" value={draftFilters.estado} onChange={(e) => setDraftFilters({ ...draftFilters, estado: e.target.value })}>{['Todos', ...filterOptions.statuses].map((item) => <option key={item} value={item}>{item === 'Todos' ? item : statusLabel(item)}</option>)}</select></FormField>
      <FormField id="texto" label="Texto libre"><input id="texto" value={draftFilters.texto} onChange={(e) => setDraftFilters({ ...draftFilters, texto: e.target.value })} placeholder="Código, nombre, descripción o tipo" /></FormField>
      <div className="filter-actions"><Button variant="primary" type="submit">Buscar</Button><Button type="button" onClick={clearFilters}>Limpiar</Button></div>
    </form>
    <div className="actions-bar" aria-label="Acciones principales">
      {canEnter ? <Button variant="primary" onClick={() => navigate('/calificaciones/nueva')}>Ingresar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => { void navigateToMockWrite('editar'); }}>Modificar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => setMessage('La eliminación real se implementará posteriormente.')}>Eliminar</Button> : null}
      {canEdit ? <Button disabled={!selected} onClick={() => { void navigateToMockWrite('copiar'); }}>Copiar</Button> : null}
      {canMassLoad ? <Button onClick={() => navigate('/cargas/x-factor')}>Carga X Factor</Button> : null}
      {canMassLoad ? <Button onClick={() => navigate('/cargas/x-monto')}>Carga X Monto</Button> : null}
    </div>
    <InlineMessage message={`${message} Mostrando ${from}–${to} de ${data.totalCount} registros.`} />
    {demoState === 'Error' && !isApi ? <ErrorState title="Error de demostración" description="Estado temporal para validar la vista de error antes de integrar la API." /> : demoState === 'Cargando' && !isApi || isLoading ? <LoadingState /> : error ? <div className="view-state view-state-error" role="alert"><strong>No fue posible cargar calificaciones</strong><span>{getUserFriendlyApiMessage(error)}</span><Button onClick={reload}>Reintentar</Button></div> : data.totalCount === 0 ? <EmptyState title="Sin resultados" description="No existen calificaciones para los filtros aplicados." actionLabel="Limpiar filtros" onAction={clearFilters} /> : <><DataTableShell records={data.items} selectedId={selected?.id ?? null} sort={sort} onSort={handleSort} onSelect={loadDetail} /><Pagination pagination={pagination} totalItems={data.totalCount} onPageChange={(page) => setPagination((current) => ({ ...current, page }))} onPageSizeChange={(pageSize) => setPagination({ page: 1, pageSize })} /></>}
    {selected ? <section className="detail-panel" aria-live="polite"><div className="detail-header"><h2>Detalle de calificación tributaria</h2><Button variant="ghost" onClick={closeDetail}>Cerrar</Button></div>{detailLoading ? <LoadingState /> : detailError ? <div className="view-state view-state-error" role="alert"><strong>No fue posible cargar el detalle</strong><span>{getUserFriendlyApiMessage(detailError)}</span></div> : detail ? <dl className="detail-grid">{getDetailFields(detail).map((field) => <div className="detail-item" key={field.label}><dt>{field.label}</dt><dd>{field.value}</dd></div>)}</dl> : null}</section> : null}
  </section>;
}

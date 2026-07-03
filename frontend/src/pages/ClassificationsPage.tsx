import {
  useEffect,
  useMemo,
  useRef,
  useState,
  type FormEvent,
  type ReactNode,
} from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  getUserFriendlyApiMessage,
  isApiError,
  ApiError,
} from "../api/client/ApiError";
import type {
  TaxClassificationDetailDto,
  TaxClassificationListRequestDto,
  TaxClassificationReadDto,
  TaxClassificationSortByDto,
  TaxClassificationSortState,
  TaxClassificationUiSortKey,
} from "../api/contracts/taxClassificationsRead";
import type { TaxClassificationHistoryDto } from "../api/contracts/taxClassificationsHistory";
import type { TaxClassificationSupervisorDecisionDto } from "../api/contracts/taxClassificationsSupervision";
import { useTaxClassificationsReadQuery } from "../api/hooks/useTaxClassificationsReadQuery";
import { useApiServices } from "../api/context/useApiServices";
import { useSession } from "../app/session/useSession";
import { Button } from "../components/Button";
import { DataTableShell } from "../components/DataTableShell";
import { EmptyState, LoadingState } from "../components/ViewStates";
import { FormField } from "../components/FormField";
import { InlineMessage } from "../components/InlineMessage";
import { Pagination } from "../components/Pagination";
import { formatIsoDateForDisplay } from "../utils/dateFormatting";
import type { PaginationState } from "../types/classification";

interface TaxClassificationReadFilters {
  mercado: string;
  ejercicio: string;
  estado: string;
  texto: string;
}
const emptyFilters: TaxClassificationReadFilters = {
  mercado: "Todos",
  ejercicio: "Todos",
  estado: "Todos",
  texto: "",
};
const supervisorDecisionOptions: Array<{
  label: string;
  value: TaxClassificationSupervisorDecisionDto;
}> = [
  { label: "Validado", value: "VALIDADO" },
  { label: "Observado", value: "OBSERVADO" },
  { label: "Aprobado", value: "APROBADO" },
];
const supervisionObservationMaxLength = 700;
type SupervisionStep = "form" | "confirmation";
const sortMap: Record<TaxClassificationUiSortKey, TaxClassificationSortByDto> =
  {
    taxPeriod: "taxPeriod",
    instrumentCode: "instrumentCode",
    validFrom: "validFrom",
    status: "status",
    market: "market",
  };
const dash = "—";
function optionalFilter(value: string) {
  return value === "Todos" ? undefined : value;
}
function selectOptions(values: Array<string | number>) {
  return ["Todos", ...values.map(String)];
}
function statusLabel(status: string) {
  return status
    .toLowerCase()
    .replace(
      /(^|\s|_|-)(\p{L})/gu,
      (_match, separator: string, letter: string) =>
        `${separator === "_" || separator === "-" ? " " : separator}${letter.toUpperCase()}`,
    );
}
function display(value: string | null | undefined) {
  return value?.trim() || dash;
}
function formatDate(value: string | null | undefined) {
  return value?.trim()
    ? formatIsoDateForDisplay(value, { includeTime: value.includes("T") })
    : dash;
}
function formatDateTimeLocal(value: string | null | undefined) {
  if (!value?.trim()) return dash;
  const parsed = new Date(value);
  if (Number.isNaN(parsed.getTime())) return dash;
  return new Intl.DateTimeFormat("es-CL", {
    dateStyle: "medium",
    timeStyle: "short",
  }).format(parsed);
}
function formatNumber(
  value: number | null | undefined,
  maximumFractionDigits = 6,
) {
  return value === null || value === undefined
    ? dash
    : value.toLocaleString("es-CL", { maximumFractionDigits });
}
function formatMoney(
  value: number | null | undefined,
  currency: string | null | undefined,
) {
  return value === null || value === undefined
    ? dash
    : new Intl.NumberFormat("es-CL", {
        style: "currency",
        currency: currency || "CLP",
        maximumFractionDigits: currency === "CLP" ? 0 : 2,
      }).format(value);
}

type DetailField = { label: string; value: ReactNode };
function getDetailFields(detail: TaxClassificationDetailDto): DetailField[] {
  return [
    { label: "Mercado", value: detail.market },
    {
      label: "Código y nombre del instrumento",
      value: `${display(detail.instrumentCode)} / ${display(detail.instrumentName)}`,
    },
    { label: "Tipo de clasificación", value: detail.classificationType },
    { label: "Descripción", value: display(detail.description) },
    {
      label: "Porcentaje de actualización",
      value: formatNumber(detail.updatePercentage),
    },
    { label: "Factor aplicado", value: formatNumber(detail.appliedFactor) },
    {
      label: "Monto de referencia y moneda",
      value: formatMoney(detail.referenceAmount, detail.currency),
    },
    { label: "Período tributario", value: detail.taxPeriod },
    { label: "Vigencia desde", value: formatDate(detail.validFrom) },
    { label: "Vigencia hasta", value: formatDate(detail.validTo) },
    { label: "Estado", value: statusLabel(detail.status) },
    { label: "Creado en", value: formatDate(detail.createdAt) },
    { label: "Actualizado en", value: formatDate(detail.updatedAt) },
  ];
}

export function ClassificationsPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useSession();
  const {
    isApi,
    taxClassificationsReadService,
    taxClassificationsSupervisionService,
    taxClassificationsHistoryService,
  } = useApiServices();
  const [draftFilters, setDraftFilters] =
    useState<TaxClassificationReadFilters>(emptyFilters);
  const [appliedFilters, setAppliedFilters] =
    useState<TaxClassificationReadFilters>(emptyFilters);
  const [sort, setSort] = useState<TaxClassificationSortState>({
    key: "taxPeriod",
    direction: "asc",
  });
  const [pagination, setPagination] = useState<PaginationState>({
    page: 1,
    pageSize: 10,
  });
  const [selected, setSelected] = useState<TaxClassificationReadDto | null>(
    null,
  );
  const [message, setMessage] = useState(
    "Seleccione un registro para consultar su detalle.",
  );
  const [detail, setDetail] = useState<TaxClassificationDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);
  const [detailError, setDetailError] = useState<ApiError | null>(null);
  const detailRequestRef = useRef(0);
  const detailControllerRef = useRef<AbortController | null>(null);
  const supervisionControllerRef = useRef<AbortController | null>(null);
  const historyControllerRef = useRef<AbortController | null>(null);
  const historyRequestRef = useRef(0);
  const [isSupervisionOpen, setIsSupervisionOpen] = useState(false);
  const [supervisionStep, setSupervisionStep] =
    useState<SupervisionStep>("form");
  const [supervisionDecision, setSupervisionDecision] = useState<
    TaxClassificationSupervisorDecisionDto | ""
  >("");
  const [supervisionObservation, setSupervisionObservation] = useState("");
  const [supervisionError, setSupervisionError] = useState("");
  const [supervisionApiError, setSupervisionApiError] =
    useState<ApiError | null>(null);
  const [supervisionSubmitting, setSupervisionSubmitting] = useState(false);
  const [isHistoryOpen, setIsHistoryOpen] = useState(false);
  const [history, setHistory] = useState<TaxClassificationHistoryDto[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historyError, setHistoryError] = useState<ApiError | null>(null);
  const request = useMemo<TaxClassificationListRequestDto>(
    () => ({
      page: pagination.page,
      pageSize: pagination.pageSize,
      search: appliedFilters.texto.trim() || undefined,
      market: optionalFilter(appliedFilters.mercado),
      exercise:
        appliedFilters.ejercicio === "Todos"
          ? undefined
          : Number(appliedFilters.ejercicio),
      status: optionalFilter(appliedFilters.estado),
      sortBy: sortMap[sort.key],
      sortDirection: sort.direction,
    }),
    [appliedFilters, pagination.page, pagination.pageSize, sort],
  );
  const { data, filterOptions, isLoading, error, reload } =
    useTaxClassificationsReadQuery(request);

  useEffect(() => {
    if (pagination.page > Math.max(1, data.totalPages))
      setPagination((current) => ({
        ...current,
        page: Math.max(1, data.totalPages),
      }));
  }, [data.totalPages, pagination.page]);
  useEffect(() => {
    if (selected && !data.items.some((record) => record.id === selected.id)) {
      setSelected(null);
      setDetail(null);
      closeSupervisionPanel();
      closeHistoryPanel();
    }
  }, [data.items, selected]);

  const canWrite =
    user?.rol === "Administrador" || user?.rol === "Analista Tributario";
  const canSupervise =
    isApi &&
    user?.rol === "Supervisor" &&
    taxClassificationsSupervisionService !== null;
  const canViewHistory =
    isApi &&
    taxClassificationsHistoryService !== null &&
    (user?.rol === "Administrador" ||
      user?.rol === "Analista Tributario" ||
      user?.rol === "Supervisor");
  const canEdit = canWrite;
  const canEnter = canEdit;
  const canMassLoad = !isApi && Boolean(user);
  const from = data.totalCount === 0 ? 0 : (data.page - 1) * data.pageSize + 1;
  const to = Math.min(data.page * data.pageSize, data.totalCount);

  useEffect(() => {
    const state = location.state as {
      taxClassificationWriteSuccess?: string;
    } | null;
    if (state?.taxClassificationWriteSuccess) {
      setMessage(state.taxClassificationWriteSuccess);
      navigate(location.pathname, { replace: true });
    }
  }, [location.pathname, location.state, navigate]);

  useEffect(
    () => () => {
      detailControllerRef.current?.abort();
      supervisionControllerRef.current?.abort();
      historyControllerRef.current?.abort();
    },
    [],
  );

  function resetSupervisionForm() {
    setSupervisionStep("form");
    setSupervisionDecision("");
    setSupervisionObservation("");
    setSupervisionError("");
    setSupervisionApiError(null);
    setSupervisionSubmitting(false);
    supervisionControllerRef.current?.abort();
  }
  function closeSupervisionPanel() {
    supervisionControllerRef.current?.abort();
    setIsSupervisionOpen(false);
    setSupervisionStep("form");
    setSupervisionError("");
    setSupervisionApiError(null);
    setSupervisionSubmitting(false);
  }
  function openSupervisionPanel() {
    if (!selected) {
      setMessage("Seleccione un registro antes de validar.");
      return;
    }
    resetSupervisionForm();
    setIsSupervisionOpen(true);
    setMessage(
      `Registro seleccionado: ${selected.id}. Complete la validación supervisora.`,
    );
  }
  function continueSupervisionConfirmation() {
    const observationLength = supervisionObservation.length;
    if (!supervisionDecision) {
      setSupervisionError("Seleccione una decisión antes de continuar.");
      return;
    }
    if (observationLength > supervisionObservationMaxLength) {
      setSupervisionError(
        `La observación no puede superar ${supervisionObservationMaxLength} caracteres.`,
      );
      return;
    }
    setSupervisionError("");
    setSupervisionApiError(null);
    setSupervisionStep("confirmation");
  }
  async function confirmSupervisionValidation() {
    if (
      !selected ||
      !taxClassificationsSupervisionService ||
      !supervisionDecision ||
      supervisionSubmitting
    )
      return;
    const controller = new AbortController();
    supervisionControllerRef.current?.abort();
    supervisionControllerRef.current = controller;
    setSupervisionSubmitting(true);
    setSupervisionApiError(null);
    try {
      const updated = await taxClassificationsSupervisionService.validate(
        selected.id,
        {
          decision: supervisionDecision,
          observation: supervisionObservation.trim() || undefined,
        },
        controller.signal,
      );
      if (controller.signal.aborted) return;
      setDetail(updated);
      setSelected(updated);
      setIsSupervisionOpen(false);
      resetSupervisionForm();
      reload();
      setMessage("Validación supervisora aplicada correctamente.");
    } catch (err: unknown) {
      if (!controller.signal.aborted)
        setSupervisionApiError(
          isApiError(err)
            ? err
            : new ApiError({
                code: "INVALID_RESPONSE",
                message: "Respuesta inválida.",
                cause: err,
              }),
        );
    } finally {
      if (!controller.signal.aborted) setSupervisionSubmitting(false);
    }
  }

  function closeHistoryPanel() {
    historyControllerRef.current?.abort();
    historyRequestRef.current += 1;
    setIsHistoryOpen(false);
    setHistory([]);
    setHistoryLoading(false);
    setHistoryError(null);
  }

  function openHistoryPanel() {
    if (!selected) {
      setMessage("Seleccione un registro antes de consultar historial.");
      return;
    }
    if (!taxClassificationsHistoryService) return;

    historyControllerRef.current?.abort();
    const controller = new AbortController();
    historyControllerRef.current = controller;
    const requestId = historyRequestRef.current + 1;
    historyRequestRef.current = requestId;
    setIsHistoryOpen(true);
    setHistory([]);
    setHistoryError(null);
    setHistoryLoading(true);
    setMessage(`Registro seleccionado: ${selected.id}. Consultando historial real.`);

    taxClassificationsHistoryService
      .getHistory(selected.id, controller.signal)
      .then((value) => {
        if (
          !controller.signal.aborted &&
          historyRequestRef.current === requestId
        ) {
          setHistory(
            [...value].sort(
              (left, right) =>
                new Date(right.changedAt).getTime() -
                new Date(left.changedAt).getTime(),
            ),
          );
        }
      })
      .catch((err: unknown) => {
        if (
          !controller.signal.aborted &&
          historyRequestRef.current === requestId
        )
          setHistoryError(
            isApiError(err)
              ? err
              : new ApiError({
                  code: "INVALID_RESPONSE",
                  message: "Respuesta inválida.",
                  cause: err,
                }),
          );
      })
      .finally(() => {
        if (
          !controller.signal.aborted &&
          historyRequestRef.current === requestId
        )
          setHistoryLoading(false);
      });
  }

  function loadDetail(record: TaxClassificationReadDto) {
    setSelected(record);
    setMessage(
      `Registro seleccionado: ${record.id}. Consultando detalle real.`,
    );
    setDetail(null);
    setDetailError(null);
    setDetailLoading(true);
    closeSupervisionPanel();
    closeHistoryPanel();
    detailControllerRef.current?.abort();
    const controller = new AbortController();
    detailControllerRef.current = controller;
    const requestId = detailRequestRef.current + 1;
    detailRequestRef.current = requestId;
    taxClassificationsReadService
      .getById(record.id, controller.signal)
      .then((value) => {
        if (
          !controller.signal.aborted &&
          detailRequestRef.current === requestId
        )
          setDetail(value);
      })
      .catch((err: unknown) => {
        if (
          !controller.signal.aborted &&
          detailRequestRef.current === requestId
        )
          setDetailError(
            isApiError(err)
              ? err
              : new ApiError({
                  code: "INVALID_RESPONSE",
                  message: "Respuesta inválida.",
                  cause: err,
                }),
          );
      })
      .finally(() => {
        if (
          !controller.signal.aborted &&
          detailRequestRef.current === requestId
        )
          setDetailLoading(false);
      });
  }
  function applyFilters(event: FormEvent) {
    event.preventDefault();
    setAppliedFilters(draftFilters);
    setPagination((current) => ({ ...current, page: 1 }));
    setSelected(null);
    setDetail(null);
    setMessage("Filtros aplicados mediante servicio de lectura.");
  }
  function clearFilters() {
    setDraftFilters(emptyFilters);
    setAppliedFilters(emptyFilters);
    setPagination((current) => ({ ...current, page: 1 }));
    setSelected(null);
    setDetail(null);
    setMessage("Filtros limpiados.");
  }
  function handleSort(key: TaxClassificationUiSortKey) {
    setSort((current) => ({
      key,
      direction:
        current.key === key && current.direction === "asc" ? "desc" : "asc",
    }));
    setPagination((current) => ({ ...current, page: 1 }));
  }
  async function navigateToMockWrite(pathSegment: "editar" | "copiar") {
    if (!selected) {
      setMessage("Seleccione un registro antes de continuar.");
      return;
    }

    const { mockClassifications } = await import("../mocks/classifications");
    const mockRecord = mockClassifications[selected.id - 1];
    if (!mockRecord) {
      setMessage(
        "No fue posible resolver el identificador histórico del registro mock seleccionado.",
      );
      return;
    }

    navigate(`/calificaciones/${mockRecord.id}/${pathSegment}`);
  }
  function navigateToApiEdit() {
    if (!selected) {
      setMessage("Seleccione un registro antes de modificar.");
      return;
    }
    navigate(`/calificaciones/${selected.id}/editar`);
  }
  function navigateToApiCopy() {
    if (!selected) {
      setMessage("Seleccione un registro antes de copiar.");
      return;
    }
    navigate(`/calificaciones/${selected.id}/copiar`);
  }
  function closeDetail() {
    detailControllerRef.current?.abort();
    closeSupervisionPanel();
    setDetail(null);
    setDetailError(null);
    setDetailLoading(false);
    setSelected(null);
    setMessage("Detalle cerrado.");
  }

  return (
    <section className="content-card classifications-page">
      {isApi ? (
        <InlineMessage message="Esta etapa integra creación, modificación y copia reales desde la API. Eliminar y cargas masivas se integrarán posteriormente." />
      ) : null}
      <form className="filters-panel" onSubmit={applyFilters}>
        <FormField id="mercado" label="Mercado">
          <select
            id="mercado"
            value={draftFilters.mercado}
            onChange={(e) =>
              setDraftFilters({ ...draftFilters, mercado: e.target.value })
            }
          >
            {selectOptions(filterOptions.markets).map((item) => (
              <option key={item}>{item}</option>
            ))}
          </select>
        </FormField>
        <FormField id="ejercicio" label="Período tributario">
          <select
            id="ejercicio"
            value={draftFilters.ejercicio}
            onChange={(e) =>
              setDraftFilters({ ...draftFilters, ejercicio: e.target.value })
            }
          >
            {selectOptions(filterOptions.exercises).map((item) => (
              <option key={item}>{item}</option>
            ))}
          </select>
        </FormField>
        <FormField id="estado" label="Estado">
          <select
            id="estado"
            value={draftFilters.estado}
            onChange={(e) =>
              setDraftFilters({ ...draftFilters, estado: e.target.value })
            }
          >
            {["Todos", ...filterOptions.statuses].map((item) => (
              <option key={item} value={item}>
                {item === "Todos" ? item : statusLabel(item)}
              </option>
            ))}
          </select>
        </FormField>
        <FormField id="texto" label="Texto libre">
          <input
            id="texto"
            value={draftFilters.texto}
            onChange={(e) =>
              setDraftFilters({ ...draftFilters, texto: e.target.value })
            }
            placeholder="Código, nombre, descripción o tipo"
          />
        </FormField>
        <div className="filter-actions">
          <Button variant="primary" type="submit">
            Buscar
          </Button>
          <Button type="button" onClick={clearFilters}>
            Limpiar
          </Button>
        </div>
      </form>
      <div className="actions-bar" aria-label="Acciones principales">
        {canEnter ? (
          <Button
            variant="primary"
            onClick={() => navigate("/calificaciones/nueva")}
          >
            Ingresar
          </Button>
        ) : null}
        {canEdit ? (
          <Button
            disabled={!selected}
            onClick={() => {
              if (isApi) navigateToApiEdit();
              else void navigateToMockWrite("editar");
            }}
          >
            Modificar
          </Button>
        ) : null}
        {!isApi && canEdit ? (
          <Button
            disabled={!selected}
            onClick={() =>
              setMessage("La eliminación real se implementará posteriormente.")
            }
          >
            Eliminar
          </Button>
        ) : null}
        {canEdit ? (
          <Button
            disabled={!selected}
            onClick={() => {
              if (isApi) navigateToApiCopy();
              else void navigateToMockWrite("copiar");
            }}
          >
            Copiar
          </Button>
        ) : null}
        {canMassLoad ? (
          <Button onClick={() => navigate("/cargas/x-factor")}>
            Carga X Factor
          </Button>
        ) : null}
        {canMassLoad ? (
          <Button onClick={() => navigate("/cargas/x-monto")}>
            Carga X Monto
          </Button>
        ) : null}
        {canSupervise ? (
          <Button disabled={!selected} onClick={openSupervisionPanel}>
            Validar calificación
          </Button>
        ) : null}
        {canViewHistory ? (
          <Button disabled={!selected} onClick={openHistoryPanel}>
            Ver historial
          </Button>
        ) : null}
      </div>
      <InlineMessage
        message={`${message} Mostrando ${from}–${to} de ${data.totalCount} registros.`}
      />
      {isLoading ? (
        <LoadingState />
      ) : error ? (
        <div className="view-state view-state-error" role="alert">
          <strong>No fue posible cargar calificaciones</strong>
          <span>{getUserFriendlyApiMessage(error)}</span>
          <Button onClick={reload}>Reintentar</Button>
        </div>
      ) : data.totalCount === 0 ? (
        <EmptyState
          title="Sin resultados"
          description="No existen calificaciones para los filtros aplicados."
          actionLabel="Limpiar filtros"
          onAction={clearFilters}
        />
      ) : (
        <>
          <DataTableShell
            records={data.items}
            selectedId={selected?.id ?? null}
            sort={sort}
            onSort={handleSort}
            onSelect={loadDetail}
          />
          <Pagination
            pagination={pagination}
            totalItems={data.totalCount}
            onPageChange={(page) =>
              setPagination((current) => ({ ...current, page }))
            }
            onPageSizeChange={(pageSize) =>
              setPagination({ page: 1, pageSize })
            }
          />
        </>
      )}

      {isSupervisionOpen && selected ? (
        <section className="supervision-panel" aria-live="polite">
          <div className="detail-header">
            <h2>Validación supervisora</h2>
            <Button
              variant="ghost"
              disabled={supervisionSubmitting}
              onClick={closeSupervisionPanel}
            >
              Cancelar
            </Button>
          </div>
          <dl className="detail-grid">
            <div className="detail-item">
              <dt>ID</dt>
              <dd>{selected.id}</dd>
            </div>
            <div className="detail-item">
              <dt>Mercado</dt>
              <dd>{selected.market}</dd>
            </div>
            <div className="detail-item">
              <dt>Código de instrumento</dt>
              <dd>{display(selected.instrumentCode)}</dd>
            </div>
            <div className="detail-item">
              <dt>Tipo de clasificación</dt>
              <dd>{selected.classificationType}</dd>
            </div>
            <div className="detail-item">
              <dt>Estado actual</dt>
              <dd>{statusLabel(selected.status)}</dd>
            </div>
          </dl>
          {supervisionApiError ? (
            <div className="view-state view-state-error" role="alert">
              <strong>No fue posible aplicar la validación</strong>
              <span>{getUserFriendlyApiMessage(supervisionApiError)}</span>
            </div>
          ) : null}
          {supervisionStep === "form" ? (
            <div className="supervision-form">
              <FormField id="supervision-decision" label="Decisión">
                <select
                  id="supervision-decision"
                  value={supervisionDecision}
                  aria-invalid={Boolean(
                    supervisionError && !supervisionDecision,
                  )}
                  onChange={(event) => {
                    setSupervisionDecision(
                      event.target.value as
                        | TaxClassificationSupervisorDecisionDto
                        | "",
                    );
                    setSupervisionError("");
                  }}
                >
                  <option value="">Seleccione una decisión</option>
                  {supervisorDecisionOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
              </FormField>
              <FormField id="supervision-observation" label="Observación">
                <textarea
                  id="supervision-observation"
                  value={supervisionObservation}
                  maxLength={supervisionObservationMaxLength + 1}
                  aria-invalid={
                    supervisionObservation.length >
                    supervisionObservationMaxLength
                  }
                  onChange={(event) => {
                    setSupervisionObservation(event.target.value);
                    setSupervisionError("");
                  }}
                />
              </FormField>
              <span className="help-text">
                {supervisionObservation.length}/
                {supervisionObservationMaxLength} caracteres
              </span>
              {supervisionError ? (
                <div className="error-summary" role="alert">
                  {supervisionError}
                </div>
              ) : null}
              <div className="form-actions">
                <Button
                  variant="primary"
                  type="button"
                  onClick={continueSupervisionConfirmation}
                >
                  Continuar a confirmación
                </Button>
                <Button type="button" onClick={closeSupervisionPanel}>
                  Cancelar
                </Button>
              </div>
            </div>
          ) : (
            <div className="supervision-confirmation">
              <h3>Confirmar validación</h3>
              <dl className="detail-grid">
                <div className="detail-item">
                  <dt>Decisión seleccionada</dt>
                  <dd>{supervisionDecision}</dd>
                </div>
                <div className="detail-item">
                  <dt>Observación</dt>
                  <dd>{supervisionObservation.trim() || "Sin observación"}</dd>
                </div>
              </dl>
              {supervisionSubmitting ? (
                <InlineMessage message="Aplicando validación..." />
              ) : null}
              <div className="form-actions">
                <Button
                  variant="primary"
                  type="button"
                  disabled={supervisionSubmitting}
                  onClick={() => void confirmSupervisionValidation()}
                >
                  Confirmar validación
                </Button>
                <Button
                  type="button"
                  disabled={supervisionSubmitting}
                  onClick={() => setSupervisionStep("form")}
                >
                  Volver
                </Button>
                <Button
                  type="button"
                  disabled={supervisionSubmitting}
                  onClick={closeSupervisionPanel}
                >
                  Cancelar
                </Button>
              </div>
            </div>
          )}
        </section>
      ) : null}
      {isHistoryOpen && selected ? (
        <section className="history-panel" aria-live="polite">
          <div className="detail-header">
            <h2>Historial de calificación tributaria</h2>
            <Button variant="ghost" onClick={closeHistoryPanel}>
              Cerrar historial
            </Button>
          </div>
          <dl className="detail-grid">
            <div className="detail-item">
              <dt>ID</dt>
              <dd>{selected.id}</dd>
            </div>
            <div className="detail-item">
              <dt>Mercado</dt>
              <dd>{selected.market}</dd>
            </div>
            <div className="detail-item">
              <dt>Código de instrumento</dt>
              <dd>{display(selected.instrumentCode)}</dd>
            </div>
            <div className="detail-item">
              <dt>Estado actual</dt>
              <dd>{statusLabel(selected.status)}</dd>
            </div>
          </dl>
          {historyLoading ? (
            <LoadingState />
          ) : historyError ? (
            <div className="view-state view-state-error" role="alert">
              <strong>No fue posible cargar el historial</strong>
              <span>{getUserFriendlyApiMessage(historyError)}</span>
            </div>
          ) : history.length === 0 ? (
            <div className="view-state">
              <strong>Sin historial</strong>
              <span>No existen eventos de historial para esta calificación tributaria.</span>
            </div>
          ) : (
            <div className="table-scroll">
              <table className="data-table history-table">
                <caption>Eventos del historial de calificación tributaria</caption>
                <thead>
                  <tr>
                    <th>Fecha y hora</th>
                    <th>Tipo de cambio</th>
                    <th>Campo modificado</th>
                    <th>Valor anterior</th>
                    <th>Valor nuevo</th>
                    <th>Observación</th>
                    <th>Usuario</th>
                  </tr>
                </thead>
                <tbody>
                  {history.map((event) => (
                    <tr key={event.id}>
                      <td>{formatDateTimeLocal(event.changedAt)}</td>
                      <td>{display(event.changeType)}</td>
                      <td>{display(event.modifiedField)}</td>
                      <td>{display(event.previousValue)}</td>
                      <td>{display(event.newValue)}</td>
                      <td>{display(event.observation)}</td>
                      <td>Usuario #{event.userId}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>
      ) : null}
      {selected ? (
        <section className="detail-panel" aria-live="polite">
          <div className="detail-header">
            <h2>Detalle de calificación tributaria</h2>
            <Button variant="ghost" onClick={closeDetail}>
              Cerrar
            </Button>
          </div>
          {detailLoading ? (
            <LoadingState />
          ) : detailError ? (
            <div className="view-state view-state-error" role="alert">
              <strong>No fue posible cargar el detalle</strong>
              <span>{getUserFriendlyApiMessage(detailError)}</span>
            </div>
          ) : detail ? (
            <dl className="detail-grid">
              {getDetailFields(detail).map((field) => (
                <div className="detail-item" key={field.label}>
                  <dt>{field.label}</dt>
                  <dd>{field.value}</dd>
                </div>
              ))}
            </dl>
          ) : null}
        </section>
      ) : null}
    </section>
  );
}

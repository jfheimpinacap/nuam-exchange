import { useCallback, useEffect, useRef, useState } from 'react';
import { ApiError, isApiError } from '../client/ApiError';
import type { ClassificationListRequestDto, ClassificationCatalogsDto } from '../contracts/classifications';
import type { PaginatedResponse } from '../contracts/common';
import { useApiServices } from '../context/useApiServices';
import type { Classification } from '../../types/classification';
const emptyData: PaginatedResponse<Classification> = { items: [], page: 1, pageSize: 10, totalItems: 0, totalPages: 0 };
const emptyCatalogs: ClassificationCatalogsDto = { markets: [], sources: [], fiscalYears: [], statuses: [] };
export function useClassificationsQuery(request: ClassificationListRequestDto) {
  const { classificationsService } = useApiServices(); const [data, setData] = useState(emptyData); const [catalogs, setCatalogs] = useState(emptyCatalogs); const [isLoading, setIsLoading] = useState(true); const [error, setError] = useState<ApiError | null>(null); const [reloadKey, setReloadKey] = useState(0); const controllerRef = useRef<AbortController | null>(null); const requestIdRef = useRef(0);
  const reload = useCallback(() => setReloadKey((value) => value + 1), []);
  useEffect(() => { const controller = new AbortController(); controllerRef.current?.abort(); controllerRef.current = controller; const requestId = requestIdRef.current + 1; requestIdRef.current = requestId; setIsLoading(true); setError(null); Promise.all([classificationsService.list(request, controller.signal), classificationsService.getCatalogs(controller.signal)]).then(([list, catalogList]) => { if (!controller.signal.aborted && requestIdRef.current === requestId) { setData(list); setCatalogs(catalogList); } }).catch((err: unknown) => { if (!controller.signal.aborted && requestIdRef.current === requestId) setError(isApiError(err) ? err : new ApiError({ code: 'INVALID_RESPONSE', message: 'Respuesta inválida.', cause: err })); }).finally(() => { if (!controller.signal.aborted && requestIdRef.current === requestId) setIsLoading(false); }); return () => controller.abort(); }, [classificationsService, request, reloadKey]);
  return { data, catalogs, isLoading, error, reload, request };
}

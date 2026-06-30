import { useCallback, useEffect, useRef, useState } from 'react';
import { ApiError, isApiError } from '../client/ApiError';
import type { TaxClassificationFilterOptionsDto, TaxClassificationListRequestDto, TaxClassificationListResponseDto } from '../contracts/taxClassificationsRead';
import { useApiServices } from '../context/useApiServices';

const emptyData: TaxClassificationListResponseDto = { items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 };
const emptyOptions: TaxClassificationFilterOptionsDto = { markets: [], exercises: [], statuses: [] };

export function useTaxClassificationsReadQuery(request: TaxClassificationListRequestDto) {
  const { taxClassificationsReadService } = useApiServices();
  const [data, setData] = useState(emptyData);
  const [filterOptions, setFilterOptions] = useState(emptyOptions);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<ApiError | null>(null);
  const [reloadKey, setReloadKey] = useState(0);
  const requestIdRef = useRef(0);
  const reload = useCallback(() => setReloadKey((value) => value + 1), []);

  useEffect(() => {
    const controller = new AbortController();
    const requestId = requestIdRef.current + 1;
    requestIdRef.current = requestId;
    setIsLoading(true); setError(null);
    Promise.all([taxClassificationsReadService.list(request, controller.signal), taxClassificationsReadService.getFilterOptions(controller.signal)])
      .then(([list, options]) => { if (!controller.signal.aborted && requestIdRef.current === requestId) { setData(list); setFilterOptions(options); } })
      .catch((err: unknown) => { if (!controller.signal.aborted && requestIdRef.current === requestId) setError(isApiError(err) ? err : new ApiError({ code: 'INVALID_RESPONSE', message: 'Respuesta inválida.', cause: err })); })
      .finally(() => { if (!controller.signal.aborted && requestIdRef.current === requestId) setIsLoading(false); });
    return () => controller.abort();
  }, [taxClassificationsReadService, request, reloadKey]);

  return { data, filterOptions, isLoading, error, reload };
}

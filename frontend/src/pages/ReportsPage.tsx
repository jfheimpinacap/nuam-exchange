import { useMemo, useState } from 'react';
import { PageHeader } from '../components/PageHeader';
import { Pagination } from '../components/Pagination';
import { mockClassifications } from '../mocks/classifications';
import { uploadReviews } from '../mocks/uploadReview';
import { ClassificationReportTable } from '../features/reports/ClassificationReportTable';
import { UploadReportTable } from '../features/reports/UploadReportTable';
import { ClassificationFilters, UploadFilters } from '../features/reports/ReportsFilters';
import { ReportsSummary } from '../features/reports/ReportsSummary';
import { ReportTypeSelector } from '../features/reports/ReportTypeSelector';
import { clsSummary, filterClassificationReport, filterUploadReport, initialClassificationFilters, initialUploadFilters, nextSort, pageInfo, sortRows, uploadSummary, validRange } from '../features/reports/reportUtils';
import { exportClassifications, exportUploads } from '../features/reports/reportExport';
import type { PaginationState } from '../types/classification';
import type { ClassificationReportFilters, ReportSortState, ReportType, UploadReportFilters } from '../types/report';
export function ReportsPage() {
  const [type, setType] = useState<ReportType>('classifications');
  const [draftC, setDraftC] = useState<ClassificationReportFilters>(initialClassificationFilters); const [activeC, setActiveC] = useState(draftC);
  const [draftU, setDraftU] = useState<UploadReportFilters>(initialUploadFilters); const [activeU, setActiveU] = useState(draftU);
  const [error, setError] = useState(''); const [pagination, setPagination] = useState<PaginationState>({ page:1, pageSize:5 });
  const [sort, setSort] = useState<ReportSortState>({ key:'fechaPago', direction:'desc' });
  const classRows = useMemo(()=>sortRows(filterClassificationReport(mockClassifications, activeC), sort), [activeC, sort]);
  const uploadRows = useMemo(()=>sortRows(filterUploadReport(uploadReviews, activeU), sort), [activeU, sort]);
  const currentTotal = type==='classifications'?classRows.length:uploadRows.length; const info = pageInfo(currentTotal, pagination.page, pagination.pageSize);
  const pagedClassRows = classRows.slice(info.start?info.start-1:0, info.end); const pagedUploadRows = uploadRows.slice(info.start?info.start-1:0, info.end);
  const generateC = () => { if(!validRange(draftC.fechaDesde,draftC.fechaHasta)){setError('Fecha desde no puede ser posterior a Fecha hasta.'); return;} setError(''); setActiveC(draftC); setPagination(p=>({...p,page:1})); };
  const generateU = () => { if(!validRange(draftU.fechaDesde,draftU.fechaHasta)){setError('Fecha desde no puede ser posterior a Fecha hasta.'); return;} setError(''); setActiveU(draftU); setPagination(p=>({...p,page:1})); };
  const onType = (v: ReportType) => { setType(v); setError(''); setPagination(p=>({...p,page:1})); setSort({ key: v==='classifications'?'fechaPago':'date', direction:'desc' }); };
  const csum=clsSummary(classRows), usum=uploadSummary(uploadRows);
  return <div className="reports-page"><PageHeader title="Reportes" description="Consulta y exportación simulada de información tributaria." /><ReportTypeSelector value={type} onChange={onType} />
    {type==='classifications' ? <><ClassificationFilters draft={draftC} error={error} onChange={setDraftC} onGenerate={generateC} onClear={()=>{setDraftC(initialClassificationFilters); setActiveC(initialClassificationFilters); setPagination(p=>({...p,page:1}));}} onExport={()=>exportClassifications(classRows)} /><ReportsSummary items={[["Total de registros",csum.total],["Monto total",csum.monto],["Vigentes",csum.Vigente],["Pendientes",csum.Pendiente],["Observadas",csum.Observada],["Rechazadas",csum.Rechazada]]} /><p aria-live="polite">Mostrando {info.start}–{info.end} de {currentTotal} registros.</p><ClassificationReportTable rows={pagedClassRows} sort={sort} onSort={(k)=>setSort(nextSort(sort,k))} /></> : <><UploadFilters draft={draftU} error={error} owners={[...new Set(uploadReviews.map(u=>u.owner))]} onChange={setDraftU} onGenerate={generateU} onClear={()=>{setDraftU(initialUploadFilters); setActiveU(initialUploadFilters); setPagination(p=>({...p,page:1}));}} onExport={()=>exportUploads(uploadRows)} /><ReportsSummary items={[["Total de cargas",usum.total],["Total de filas",usum.totalRows],["Filas válidas",usum.valid],["Filas con error",usum.invalid],["Porcentaje válido",usum.pct]]} /><p aria-live="polite">Mostrando {info.start}–{info.end} de {currentTotal} registros.</p><UploadReportTable rows={pagedUploadRows} sort={sort} onSort={(k)=>setSort(nextSort(sort,k))} /></>}
    <Pagination pagination={pagination} totalItems={currentTotal} onPageChange={(page)=>setPagination(p=>({...p,page}))} onPageSizeChange={(pageSize)=>setPagination({page:1,pageSize})} />
  </div>;
}

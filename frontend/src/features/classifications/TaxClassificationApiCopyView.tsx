import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getUserFriendlyApiMessage } from '../../api/client/ApiError';
import { useApiServices } from '../../api/context/useApiServices';
import type { TaxClassificationDetailDto } from '../../api/contracts/taxClassificationsRead';
import { Button } from '../../components/Button';
import { InlineMessage } from '../../components/InlineMessage';
import { formatIsoDateForDisplay } from '../../utils/dateFormatting';

type CopyField = { label: string; value: string };
const dash = '—';
function display(value: string | null | undefined) { return value?.trim() || dash; }
function formatDate(value: string | null | undefined) { return value?.trim() ? formatIsoDateForDisplay(value, { includeTime: value.includes('T') }) : dash; }
function statusLabel(status: string) { return status.toLowerCase().replace(/(^|\s|_|-)(\p{L})/gu, (_match, separator: string, letter: string) => `${separator === '_' || separator === '-' ? ' ' : separator}${letter.toUpperCase()}`); }
function getCopyFields(detail: TaxClassificationDetailDto): CopyField[] {
  return [
    { label: 'Mercado', value: detail.market },
    { label: 'Código y nombre del instrumento', value: `${display(detail.instrumentCode)} / ${display(detail.instrumentName)}` },
    { label: 'Tipo de clasificación', value: detail.classificationType },
    { label: 'Descripción', value: display(detail.description) },
    { label: 'Período tributario', value: String(detail.taxPeriod) },
    { label: 'Vigencia desde', value: formatDate(detail.validFrom) },
    { label: 'Vigencia hasta', value: formatDate(detail.validTo) },
    { label: 'Estado', value: statusLabel(detail.status) },
  ];
}

interface Props { id: number; detail: TaxClassificationDetailDto; }
export function TaxClassificationApiCopyView({ id, detail }: Props) {
  const navigate = useNavigate();
  const { taxClassificationsWriteService } = useApiServices();
  const [confirming, setConfirming] = useState(false);
  const [saving, setSaving] = useState(false);
  const [apiError, setApiError] = useState('');
  const copyControllerRef = useRef<AbortController | null>(null);

  useEffect(() => () => copyControllerRef.current?.abort(), []);

  async function confirmCopy() {
    if (saving || !taxClassificationsWriteService) return;
    copyControllerRef.current?.abort();
    const controller = new AbortController();
    copyControllerRef.current = controller;
    setSaving(true);
    setApiError('');
    try {
      const created = await taxClassificationsWriteService.copy(id, controller.signal);
      navigate(`/calificaciones/${created.id}/editar`, { state: { taxClassificationWriteSuccess: 'Copia creada correctamente. Revise y ajuste la nueva calificación antes de continuar.' } });
    } catch (error) {
      if (!controller.signal.aborted) {
        setApiError(getUserFriendlyApiMessage(error));
        setSaving(false);
      }
    }
  }

  return <section className="content-card classification-copy-page">
    <header className="page-header"><p className="eyebrow">Calificaciones Tributarias</p><h1>Copiar Calificación Tributaria</h1><p>Revise el registro origen antes de crear una copia real.</p></header>
    <InlineMessage tone="warning" message="La copia se creará inmediatamente mediante la API. Después podrá modificar el nuevo registro." />
    {apiError ? <InlineMessage tone="error" message={apiError} /> : null}
    <dl className="detail-grid copy-confirmation-grid">{getCopyFields(detail).map((field) => <div className="detail-item" key={field.label}><dt>{field.label}</dt><dd>{field.value}</dd></div>)}</dl>
    {confirming ? <div className="copy-confirmation-panel" role="alert"><strong>Confirme la creación de la copia real.</strong><p>Esta acción enviará la solicitud de copia a la API para el registro {id}.</p><div className="filter-actions"><Button variant="primary" type="button" onClick={() => { void confirmCopy(); }} disabled={saving}>{saving ? 'Creando copia...' : 'Confirmar copia'}</Button><Button type="button" onClick={() => setConfirming(false)} disabled={saving}>Cancelar</Button></div></div> : null}
    <div className="form-actions"><Button variant="primary" type="button" onClick={() => { setConfirming(true); setApiError(''); }} disabled={saving || confirming}>Crear copia</Button><Button type="button" onClick={() => navigate('/calificaciones')} disabled={saving}>Cancelar</Button></div>
  </section>;
}

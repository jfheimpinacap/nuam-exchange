import { useState } from 'react';
import { Link } from 'react-router-dom';
import { Button } from '../components/Button';
import { InlineMessage } from '../components/InlineMessage';
import { PageHeader } from '../components/PageHeader';
import { useApiServices } from '../api/context/useApiServices';
import type { UploadType } from '../types/upload';
import { uploadConfigs } from '../features/uploads/uploadConfigs';
import { downloadTemplate } from '../features/uploads/uploadTemplateUtils';
import { downloadCsvTemplate } from '../utils/downloadCsvTemplate';

interface ApiTemplateConfig {
  title: string;
  fileName: string;
  header: string;
  uploadPath: string;
  downloadLabel: string;
  uploadLabel: string;
}

const windowsLineBreak = '\r\n';

const apiTemplates: ApiTemplateConfig[] = [
  {
    title: 'Carga X Factor',
    fileName: 'plantilla-carga-x-factor.csv',
    header: 'market;instrumentCode;taxPeriod;appliedFactor',
    uploadPath: '/cargas/x-factor',
    downloadLabel: 'Descargar plantilla X Factor',
    uploadLabel: 'Ir a Carga X Factor',
  },
  {
    title: 'Carga X Monto',
    fileName: 'plantilla-carga-x-monto.csv',
    header: 'market;instrumentCode;taxPeriod;referenceAmount',
    uploadPath: '/cargas/x-monto',
    downloadLabel: 'Descargar plantilla X Monto',
    uploadLabel: 'Ir a Carga X Monto',
  },
];

function downloadApiTemplate(template: ApiTemplateConfig): void {
  downloadCsvTemplate(template.fileName, `${template.header}${windowsLineBreak}`);
}

function ApiUploadTemplatesPage() {
  return (
    <div className="templates-page">
      <PageHeader
        title="Plantillas de carga"
        description="Descargue la plantilla correspondiente, complete al menos una fila de datos válida y luego utilice la carga masiva asociada."
      />
      <InlineMessage
        tone="warning"
        message="Las plantillas incluyen únicamente el encabezado. Debe agregar al menos una fila de datos antes de ejecutar una carga masiva."
      />
      <div className="template-cards-grid">
        {apiTemplates.map((template) => (
          <section className="template-card" key={template.fileName}>
            <h2>{template.title}</h2>
            <dl className="template-detail-list">
              <div>
                <dt>Archivo</dt>
                <dd>{template.fileName}</dd>
              </div>
              <div>
                <dt>Encabezado</dt>
                <dd><code>{template.header}</code></dd>
              </div>
            </dl>
            <div className="filter-actions">
              <Button variant="primary" onClick={() => downloadApiTemplate(template)}>
                {template.downloadLabel}
              </Button>
              <Link className="primary-link" to={template.uploadPath}>{template.uploadLabel}</Link>
            </div>
          </section>
        ))}
      </div>
    </div>
  );
}

export function UploadTemplatesPage(){ const { isApi } = useApiServices(); const [type,setType]=useState<UploadType>('x-factor'); const config=uploadConfigs[type]; if (isApi) return <ApiUploadTemplatesPage />; return <div className="templates-page"><PageHeader title="Plantillas de carga" description="Consulta y descarga de formatos CSV provisionales." /><InlineMessage tone="warning" message="Plantilla provisional para pruebas de interfaz. El formato definitivo dependerá del contrato del backend." /><div className="tabs" role="tablist" aria-label="Formatos de carga">{(['x-factor','x-monto'] as UploadType[]).map((t)=><button key={t} type="button" role="tab" aria-selected={type===t} className="button button-secondary" onClick={()=>setType(t)}>{uploadConfigs[t].shortTitle}</button>)}</div><section className="template-card"><h2>{config.shortTitle}</h2><div className="table-scroll"><table className="data-table"><caption>Formato provisional {config.shortTitle}</caption><thead><tr><th>Orden</th><th>Nombre de columna</th><th>Obligatorio</th><th>Tipo esperado</th><th>Ejemplo</th><th>Regla de validación</th></tr></thead><tbody>{config.columns.map((c,i)=><tr key={c.key}><td>{i+1}</td><td>{c.key}</td><td>{c.required?'Sí':'No'}</td><td>{c.type}</td><td>{c.example}</td><td>{c.rule}</td></tr>)}</tbody></table></div><h3>Vista previa</h3><pre className="csv-preview">{[config.columns.map((c)=>c.key),...config.examples].map((r)=>r.join(';')).join('\n')}</pre><div className="filter-actions"><Button variant="primary" onClick={()=>downloadTemplate(config)}>Descargar plantilla CSV</Button><Link className="primary-link" to="/cargas/x-factor">Ir a Carga X Factor</Link><Link className="primary-link" to="/cargas/x-monto">Ir a Carga X Monto</Link></div></section></div>; }

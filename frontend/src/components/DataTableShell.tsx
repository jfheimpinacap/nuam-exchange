import type { TaxClassificationReadDto } from '../api/contracts/taxClassificationsRead';
import type { TaxClassificationSortState, TaxClassificationUiSortKey } from '../api/contracts/taxClassificationsRead';
import { Button } from './Button';
import { StatusBadge } from './StatusBadge';

interface DataTableShellProps {
  records: TaxClassificationReadDto[];
  selectedId: number | null;
  sort: TaxClassificationSortState;
  onSort: (key: TaxClassificationUiSortKey) => void;
  onSelect: (record: TaxClassificationReadDto) => void;
}

const sortableColumns: Array<{ key: TaxClassificationUiSortKey; label: string }> = [
  { key: 'taxPeriod', label: 'Período tributario' },
  { key: 'instrumentCode', label: 'Código de instrumento' },
  { key: 'validFrom', label: 'Vigencia desde' },
  { key: 'status', label: 'Estado' },
  { key: 'market', label: 'Mercado' },
];

const dash = '—';
function formatDate(value: string | null | undefined) { if (!value) return dash; const date = new Date(value); return Number.isNaN(date.getTime()) ? value : new Intl.DateTimeFormat('es-CL', { dateStyle: 'medium' }).format(date); }
function formatNumber(value: number | null | undefined, maximumFractionDigits = 6) { return value === null || value === undefined ? dash : value.toLocaleString('es-CL', { maximumFractionDigits }); }
function formatMoney(value: number | null, currency: string) { return value === null ? dash : new Intl.NumberFormat('es-CL', { style: 'currency', currency, maximumFractionDigits: currency === 'CLP' ? 0 : 2 }).format(value); }
function display(value: string | null | undefined) { return value?.trim() || dash; }
function sortLabel(key: TaxClassificationUiSortKey, sort: TaxClassificationSortState) { if (sort.key !== key) return 'Sin ordenar'; return sort.direction === 'asc' ? 'Orden ascendente' : 'Orden descendente'; }

export function DataTableShell({ records, selectedId, sort, onSort, onSelect }: DataTableShellProps) {
  const ariaSort = (key: TaxClassificationUiSortKey) => sort.key === key ? (sort.direction === 'asc' ? 'ascending' : 'descending') : 'none';
  return <div className="table-scroll">
    <table className="data-table">
      <caption>Listado de calificaciones tributarias obtenidas desde el servicio configurado.</caption>
      <thead><tr>
        <th scope="col">Selección</th>
        {sortableColumns.map((column) => <th key={column.key} scope="col" aria-sort={ariaSort(column.key)}><button className="sortable-heading" type="button" onClick={() => onSort(column.key)}>{column.label}<span>{sortLabel(column.key, sort)}</span></button></th>)}
        <th scope="col">Monto de referencia</th><th scope="col">Tipo de clasificación</th><th scope="col">Descripción</th><th scope="col">Factor aplicado</th><th scope="col">Acción</th>
      </tr></thead>
      <tbody>{records.map((record) => {
        const selected = selectedId === record.id;
        return <tr key={record.id} className={selected ? 'is-selected' : ''} aria-selected={selected} onClick={() => onSelect(record)}>
          <td><input type="radio" name="selected-classification" checked={selected} onChange={() => onSelect(record)} aria-label={`Seleccionar ${display(record.instrumentCode)}`} /></td>
          <td>{record.taxPeriod}</td><td>{display(record.instrumentCode)}</td><td>{formatDate(record.validFrom)}</td><td><StatusBadge status={record.status} /></td><td>{record.market}</td><td>{formatMoney(record.referenceAmount, record.currency)}</td><td>{record.classificationType}</td><td>{display(record.description)}</td><td>{formatNumber(record.appliedFactor)}</td><td><Button variant="ghost" onClick={(event) => { event.stopPropagation(); onSelect(record); }}>Ver detalle</Button></td>
        </tr>;
      })}</tbody>
    </table>
  </div>;
}

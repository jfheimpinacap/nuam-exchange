import type { Classification, ClassificationSortKey, SortState } from '../types/classification';
import { Button } from './Button';
import { StatusBadge } from './StatusBadge';

interface DataTableShellProps {
  records: Classification[];
  selectedId: string | null;
  sort: SortState;
  onSort: (key: ClassificationSortKey) => void;
  onSelect: (record: Classification) => void;
}

const sortableColumns: Array<{ key: ClassificationSortKey; label: string }> = [
  { key: 'ejercicio', label: 'Ejercicio' },
  { key: 'instrumento', label: 'Instrumento' },
  { key: 'fechaPago', label: 'Fecha de pago' },
  { key: 'monto', label: 'Monto' },
  { key: 'estado', label: 'Estado' },
];

const moneyFormatter = new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP', maximumFractionDigits: 0 });

function sortLabel(key: ClassificationSortKey, sort: SortState) {
  if (sort.key !== key) return 'Sin ordenar';
  return sort.direction === 'asc' ? 'Orden ascendente' : 'Orden descendente';
}

export function DataTableShell({ records, selectedId, sort, onSort, onSelect }: DataTableShellProps) {
  const ariaSort = (key: ClassificationSortKey) => sort.key === key ? (sort.direction === 'asc' ? 'ascending' : 'descending') : 'none';
  return <div className="table-scroll">
    <table className="data-table">
      <caption>Listado administrativo de calificaciones tributarias mockeadas.</caption>
      <thead><tr>
        <th scope="col">Selección</th>
        {sortableColumns.map((column) => <th key={column.key} scope="col" aria-sort={ariaSort(column.key)}>
          <button className="sortable-heading" type="button" onClick={() => onSort(column.key)}>
            {column.label}<span>{sortLabel(column.key, sort)}</span>
          </button>
        </th>)}
        <th scope="col">Mercado</th><th scope="col">Origen</th><th scope="col">Descripción</th><th scope="col">Secuencia de evento</th><th scope="col">Factor de actualización</th><th scope="col">Acción</th>
      </tr></thead>
      <tbody>{records.map((record) => {
        const selected = selectedId === record.id;
        return <tr key={record.id} className={selected ? 'is-selected' : ''} aria-selected={selected} onClick={() => onSelect(record)}>
          <td><input type="radio" name="selected-classification" checked={selected} onChange={() => onSelect(record)} aria-label={`Seleccionar ${record.instrumento}`} /></td>
          <td>{record.ejercicio}</td><td>{record.instrumento}</td><td>{record.fechaPago}</td><td>{moneyFormatter.format(record.monto)}</td><td><StatusBadge status={record.estado} /></td><td>{record.mercado}</td><td>{record.origen}</td><td>{record.descripcion}</td><td>{record.secuenciaEvento}</td><td>{record.factorActualizacion.toLocaleString('es-CL', { maximumFractionDigits: 6 })}</td><td><Button variant="ghost" onClick={(event) => { event.stopPropagation(); onSelect(record); }}>Ver</Button></td>
        </tr>;
      })}</tbody>
    </table>
  </div>;
}

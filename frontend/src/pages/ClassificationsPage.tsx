import { useEffect, useMemo, useState } from 'react';
import { listClassifications } from '../api/services';
import { ClassificationsTable } from '../features/classifications/ClassificationsTable';
import type { Classification } from '../types';

export function ClassificationsPage() {
  const [rows, setRows] = useState<Classification[]>([]);
  const [query, setQuery] = useState('');
  useEffect(() => { void listClassifications().then(setRows); }, []);
  const filtered = useMemo(() => rows.filter((row) => `${row.instrument} ${row.description} ${row.market}`.toLowerCase().includes(query.toLowerCase())), [query, rows]);
  return <section className="page"><h1>Calificaciones tributarias</h1><div className="actions"><input placeholder="Buscar instrumento, mercado o descripción" value={query} onChange={(event) => setQuery(event.target.value)} /><button type="button">Nueva calificación visual</button><button type="button">Carga masiva mock</button></div><ClassificationsTable rows={filtered} /></section>;
}

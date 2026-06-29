import { useEffect, useRef } from 'react';
import { Button } from '../../components/Button';
import { FormField } from '../../components/FormField';
import type { AuditFilters as Filters } from '../../types/audit';
import { auditActions, auditModules, auditResults, auditSeverities } from './auditUtils';
import type { UserRole } from '../../types/session';
export function AuditFilters({ draft, users, error, onChange, onSearch, onClear, onExport }: { draft:Filters; users:string[]; error:string; onChange:(f:Filters)=>void; onSearch:()=>void; onClear:()=>void; onExport:()=>void }) { const errorRef=useRef<HTMLParagraphElement>(null); useEffect(()=>{ if(error) errorRef.current?.focus(); },[error]); const roles: UserRole[]=['Administrador','Analista Tributario','Supervisor']; return <section className="filters-panel audit-filters" aria-label="Filtros de auditoría">
<FormField id="audit-text" label="Texto libre"><input id="audit-text" value={draft.texto} onChange={e=>onChange({...draft,texto:e.target.value})} /></FormField>
<FormField id="audit-from" label="Fecha desde"><input id="audit-from" type="date" value={draft.desde} aria-invalid={Boolean(error)} onChange={e=>onChange({...draft,desde:e.target.value})} /></FormField>
<FormField id="audit-to" label="Fecha hasta"><input id="audit-to" type="date" value={draft.hasta} aria-invalid={Boolean(error)} onChange={e=>onChange({...draft,hasta:e.target.value})} /></FormField>
<FormField id="audit-user" label="Usuario"><select id="audit-user" value={draft.usuario} onChange={e=>onChange({...draft,usuario:e.target.value})}><option>Todos</option>{users.map(u=><option key={u}>{u}</option>)}</select></FormField>
<FormField id="audit-role" label="Rol"><select id="audit-role" value={draft.rol} onChange={e=>onChange({...draft,rol:e.target.value as Filters['rol']})}><option>Todos</option>{roles.map(r=><option key={r}>{r}</option>)}</select></FormField>
<FormField id="audit-module" label="Módulo"><select id="audit-module" value={draft.modulo} onChange={e=>onChange({...draft,modulo:e.target.value as Filters['modulo']})}><option>Todos</option>{auditModules.map(v=><option key={v}>{v}</option>)}</select></FormField>
<FormField id="audit-action" label="Acción"><select id="audit-action" value={draft.accion} onChange={e=>onChange({...draft,accion:e.target.value as Filters['accion']})}><option>Todos</option>{auditActions.map(v=><option key={v}>{v}</option>)}</select></FormField>
<FormField id="audit-result" label="Resultado"><select id="audit-result" value={draft.resultado} onChange={e=>onChange({...draft,resultado:e.target.value as Filters['resultado']})}><option>Todos</option>{auditResults.map(v=><option key={v}>{v}</option>)}</select></FormField>
<FormField id="audit-severity" label="Severidad"><select id="audit-severity" value={draft.severidad} onChange={e=>onChange({...draft,severidad:e.target.value as Filters['severidad']})}><option>Todos</option>{auditSeverities.map(v=><option key={v}>{v}</option>)}</select></FormField>
<div className="filter-actions"><Button variant="primary" onClick={onSearch}>Buscar</Button><Button onClick={onClear}>Limpiar</Button><Button onClick={onExport}>Exportar CSV</Button></div>{error&&<p ref={errorRef} tabIndex={-1} className="field-error" role="alert">{error}</p>}</section>; }

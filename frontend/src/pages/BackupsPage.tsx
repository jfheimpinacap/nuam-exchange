import { useState } from 'react';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { PageHeader } from '../components/PageHeader';
import { StatusBadge } from '../components/StatusBadge';
import { backupRecords } from '../mocks/classifications';
export function BackupsPage() { const [confirm,setConfirm]=useState(false); return <section className="page"><PageHeader title="Respaldos" description="Gestión visual simulada de respaldos y restauraciones." /><div className="actions"><button type="button" onClick={()=>setConfirm(true)}>Crear respaldo visual</button></div><table><tbody>{backupRecords.map(item=><tr key={item.id}><td>{item.name}</td><td>{item.date}</td><td><StatusBadge value={item.status} /></td><td>{item.size}</td></tr>)}</tbody></table><ConfirmDialog open={confirm} title="Crear respaldo" onCancel={()=>setConfirm(false)} onConfirm={()=>setConfirm(false)}>Esta acción es solo una simulación en memoria.</ConfirmDialog></section>; }

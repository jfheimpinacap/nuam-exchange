import { useEffect, useRef } from 'react';
import { Button } from './Button';
export function ConfirmDialog({ title, message, confirmLabel='Confirmar', onConfirm, onClose }: { title:string; message:string; confirmLabel?:string; onConfirm:()=>void; onClose:()=>void }) {
 const ref=useRef<HTMLButtonElement>(null); useEffect(()=>{ref.current?.focus(); const onKey=(e:KeyboardEvent)=>{ if(e.key==='Escape') onClose();}; window.addEventListener('keydown',onKey); return()=>window.removeEventListener('keydown',onKey);},[onClose]);
 return <div className="dialog-backdrop"><section className="confirm-dialog" role="dialog" aria-modal="true" aria-labelledby="confirm-title"><h2 id="confirm-title">{title}</h2><p>{message}</p><div className="filter-actions"><Button variant="primary" onClick={onConfirm}>{confirmLabel}</Button><Button ref={ref} onClick={onClose}>Cancelar</Button></div></section></div>;
}

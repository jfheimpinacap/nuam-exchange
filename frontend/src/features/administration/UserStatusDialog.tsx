import { useEffect } from 'react';
import { Button } from '../../components/Button';
import type { AdministrationUser, UserAccountStatus } from '../../types/administration';
export function UserStatusDialog({ user, status, onConfirm, onClose }: { user: AdministrationUser; status: UserAccountStatus; onConfirm: () => void; onClose: () => void }) {
  useEffect(() => { const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); }; document.addEventListener('keydown', onKey); return () => document.removeEventListener('keydown', onKey); }, [onClose]);
  return <div className="dialog-backdrop"><section className="confirm-dialog" role="dialog" aria-modal="true" aria-labelledby="status-title"><h2 id="status-title">Confirmar cambio de estado</h2><p>El usuario <strong>{user.nombre}</strong> quedará con estado <strong>{status}</strong>. Este cambio será solo en memoria.</p><div className="filter-actions"><Button variant="primary" onClick={onConfirm}>Confirmar</Button><Button onClick={onClose}>Cancelar</Button></div></section></div>;
}

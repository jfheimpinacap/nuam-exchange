import { useEffect } from 'react';
import { Button } from '../../components/Button';
import type { AdministrationUser } from '../../types/administration';
export function ResetAccessDialog({ user, onConfirm, onClose }: { user: AdministrationUser; onConfirm: () => void; onClose: () => void }) {
  useEffect(() => { const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') onClose(); }; document.addEventListener('keydown', onKey); return () => document.removeEventListener('keydown', onKey); }, [onClose]);
  return <div className="dialog-backdrop"><section className="confirm-dialog" role="dialog" aria-modal="true" aria-labelledby="reset-title"><h2 id="reset-title">Restablecer acceso</h2><p>Se simulará el restablecimiento de acceso para <strong>{user.nombre}</strong>. No se generarán contraseñas, credenciales ni correos.</p><div className="filter-actions"><Button variant="primary" onClick={onConfirm}>Confirmar</Button><Button onClick={onClose}>Cancelar</Button></div></section></div>;
}

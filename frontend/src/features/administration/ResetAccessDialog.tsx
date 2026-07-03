import { useState } from 'react';
import { Button } from '../../components/Button';
import { FormField } from '../../components/FormField';
import type { AdministrationUser } from '../../types/administration';
export function ResetAccessDialog({ user, onConfirm, onClose }: { user: AdministrationUser; onConfirm: (password: string) => void; onClose: () => void }) {
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const error = password && confirm && password !== confirm ? 'La confirmación no coincide con la contraseña.' : '';
  return <div className="dialog-backdrop"><section className="confirm-dialog" role="dialog" aria-modal="true" aria-labelledby="reset-title"><h2 id="reset-title">Restablecer acceso</h2><p>Define una nueva contraseña para <strong>{user.nombre}</strong>. No se enviarán correos ni se mostrará la contraseña.</p><div className="dialog-form"><FormField id="reset-password" label="Nueva contraseña"><input id="reset-password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} /><small>Mínimo 8 caracteres; debe cumplir la política de seguridad configurada en la API.</small></FormField><FormField id="reset-confirm" label="Confirmación de nueva contraseña"><input id="reset-confirm" type="password" value={confirm} onChange={(e) => setConfirm(e.target.value)} />{error ? <span className="field-error" role="alert">{error}</span> : null}</FormField></div><div className="filter-actions"><Button variant="primary" disabled={!password || !confirm || Boolean(error)} onClick={() => onConfirm(password)}>Confirmar</Button><Button onClick={onClose}>Cancelar</Button></div></section></div>;
}

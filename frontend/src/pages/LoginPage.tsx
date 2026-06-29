import { FormEvent, useMemo, useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import { mockUsers } from '../mocks/users';

export function LoginPage() {
  const navigate = useNavigate();
  const { isAuthenticated, login } = useSession();
  const [selectedUserId, setSelectedUserId] = useState('');
  const [password, setPassword] = useState('');
  const [submitted, setSubmitted] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);

  const selectedUser = useMemo(
    () => mockUsers.find((user) => user.id === selectedUserId) ?? null,
    [selectedUserId],
  );
  const profileError = submitted && !selectedUser ? 'Selecciona un perfil de demostración.' : '';
  const passwordError = submitted && !password.trim() ? 'Ingresa una contraseña para continuar la simulación.' : '';
  const isFormValid = Boolean(selectedUser && password.trim());

  if (isAuthenticated) {
    return <Navigate to="/inicio" replace />;
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitted(true);

    if (!selectedUser || !password.trim()) {
      return;
    }

    setIsProcessing(true);
    window.setTimeout(() => {
      login(selectedUser);
      setPassword('');
      navigate('/inicio', { replace: true });
    }, 150);
  };

  return (
    <main className="login-page">
      <section className="login-card" aria-labelledby="login-title">
        <p className="eyebrow">Nuam Exchange</p>
        <h1 id="login-title">Sistema de Gestión Tributaria</h1>
        <p>Acceso de demostración con sesión simulada en memoria, sin conexión al backend.</p>
        <form className="login-form" onSubmit={handleSubmit} noValidate>
          <label htmlFor="demo-profile">
            Perfil de demostración
            <select
              id="demo-profile"
              value={selectedUserId}
              onChange={(event) => setSelectedUserId(event.target.value)}
              aria-invalid={Boolean(profileError)}
              aria-describedby={profileError ? 'profile-error' : undefined}
            >
              <option value="">Selecciona un perfil</option>
              {mockUsers.map((user) => (
                <option key={user.id} value={user.id}>{user.nombre} — {user.rol}</option>
              ))}
            </select>
          </label>
          {profileError ? <p className="field-error" id="profile-error" role="alert">{profileError}</p> : null}
          <label htmlFor="demo-password">
            Contraseña visual
            <input
              id="demo-password"
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Ingresa cualquier texto"
              aria-invalid={Boolean(passwordError)}
              aria-describedby={passwordError ? 'password-error' : undefined}
            />
          </label>
          {passwordError ? <p className="field-error" id="password-error" role="alert">{passwordError}</p> : null}
          <p className="login-info">Esta pantalla no valida credenciales reales, no guarda contraseñas y no emite tokens.</p>
          <button className="primary-button" type="submit" disabled={!isFormValid || isProcessing}>
            {isProcessing ? 'Procesando…' : 'Ingresar'}
          </button>
        </form>
      </section>
    </main>
  );
}

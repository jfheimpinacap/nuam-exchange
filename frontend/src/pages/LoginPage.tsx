import { FormEvent, useMemo, useState } from 'react';
import { Navigate, useNavigate } from 'react-router-dom';
import { useApiServices } from '../api/context/useApiServices';
import { useSession } from '../app/session/useSession';
import { mockUsers } from '../mocks/users';

const apiLoginErrorMessage = 'No fue posible iniciar sesión. Verifica tus credenciales o la disponibilidad del servicio.';

export function LoginPage() {
  const navigate = useNavigate();
  const { isApi } = useApiServices();
  const { isAuthenticated, login, loginWithApi, status } = useSession();
  const [selectedUserId, setSelectedUserId] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [submitted, setSubmitted] = useState(false);
  const [isProcessing, setIsProcessing] = useState(false);
  const [formError, setFormError] = useState('');

  const selectedUser = useMemo(
    () => mockUsers.find((user) => user.id === selectedUserId) ?? null,
    [selectedUserId],
  );
  const trimmedEmail = email.trim();
  const trimmedPassword = password.trim();
  const profileError = !isApi && submitted && !selectedUser ? 'Selecciona un perfil de demostración.' : '';
  const emailError = isApi && submitted && !trimmedEmail ? 'Ingresa tu email.' : '';
  const passwordError = submitted && !trimmedPassword
    ? isApi ? 'Ingresa tu contraseña.' : 'Ingresa una contraseña para continuar la simulación.'
    : '';
  const isFormValid = isApi
    ? Boolean(trimmedEmail && trimmedPassword)
    : Boolean(selectedUser && trimmedPassword);

  if (status === 'restoring') {
    return <main className="login-page"><p className="page-status">Restaurando sesión…</p></main>;
  }

  if (isAuthenticated) {
    return <Navigate to="/inicio" replace />;
  }

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setSubmitted(true);
    setFormError('');

    if (!isFormValid) {
      return;
    }

    setIsProcessing(true);

    if (!isApi) {
      window.setTimeout(() => {
        if (selectedUser) {
          login(selectedUser);
          setPassword('');
          navigate('/inicio', { replace: true });
        }
        setIsProcessing(false);
      }, 150);
      return;
    }

    const result = await loginWithApi({ email: trimmedEmail, password });
    setIsProcessing(false);

    if (result.ok) {
      setPassword('');
      navigate('/inicio', { replace: true });
      return;
    }

    setFormError(result.error ?? apiLoginErrorMessage);
  };

  return (
    <main className="login-page">
      <section className="login-card" aria-labelledby="login-title">
        <p className="eyebrow">Nuam Exchange</p>
        <h1 id="login-title">Sistema de Gestión Tributaria</h1>
        <p>{isApi ? 'Ingresa con tus credenciales institucionales.' : 'Acceso de demostración con sesión simulada en memoria, sin conexión al backend.'}</p>
        <form className="login-form" onSubmit={handleSubmit} noValidate>
          {isApi ? (
            <>
              <label htmlFor="login-email">
                Email
                <input
                  id="login-email"
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  autoComplete="username"
                  aria-invalid={Boolean(emailError)}
                  aria-describedby={emailError ? 'email-error' : undefined}
                  disabled={isProcessing}
                />
              </label>
              {emailError ? <p className="field-error" id="email-error" role="alert">{emailError}</p> : null}
            </>
          ) : (
            <>
              <label htmlFor="demo-profile">
                Perfil de demostración
                <select
                  id="demo-profile"
                  value={selectedUserId}
                  onChange={(event) => setSelectedUserId(event.target.value)}
                  aria-invalid={Boolean(profileError)}
                  aria-describedby={profileError ? 'profile-error' : undefined}
                  disabled={isProcessing}
                >
                  <option value="">Selecciona un perfil</option>
                  {mockUsers.map((user) => (
                    <option key={user.id} value={user.id}>{user.nombre} — {user.rol}</option>
                  ))}
                </select>
              </label>
              {profileError ? <p className="field-error" id="profile-error" role="alert">{profileError}</p> : null}
            </>
          )}
          <label htmlFor={isApi ? 'login-password' : 'demo-password'}>
            {isApi ? 'Contraseña' : 'Contraseña visual'}
            <input
              id={isApi ? 'login-password' : 'demo-password'}
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder={isApi ? undefined : 'Ingresa cualquier texto'}
              autoComplete={isApi ? 'current-password' : undefined}
              aria-invalid={Boolean(passwordError)}
              aria-describedby={passwordError ? 'password-error' : undefined}
              disabled={isProcessing}
            />
          </label>
          {passwordError ? <p className="field-error" id="password-error" role="alert">{passwordError}</p> : null}
          {formError ? <p className="field-error" role="alert">{formError}</p> : null}
          {!isApi ? <p className="login-info">Esta pantalla no valida credenciales reales, no guarda contraseñas y no emite tokens.</p> : null}
          <button className="primary-button" type="submit" disabled={!isFormValid || isProcessing}>
            {isProcessing ? 'Procesando…' : 'Ingresar'}
          </button>
        </form>
      </section>
    </main>
  );
}

import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import './styles.css';

function App() {
  return (
    <main className="shell" aria-labelledby="page-title">
      <section className="card" aria-describedby="project-description">
        <p className="eyebrow">Nuam Exchange</p>
        <h1 id="page-title">Sistema de Gestión Tributaria</h1>
        <p id="project-description">Base técnica inicial del proyecto</p>
        <p className="status">Estado: frontend preparado para integración con API</p>
      </section>
    </main>
  );
}

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <App />
  </StrictMode>
);

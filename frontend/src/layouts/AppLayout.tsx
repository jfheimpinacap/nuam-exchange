import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Breadcrumbs } from '../components/Breadcrumbs';
import { Header } from '../components/Header';
import { Sidebar } from '../components/Sidebar';

export function AppLayout() {
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  return (
    <div className="app-shell">
      <Sidebar
        isMobileOpen={isMobileOpen}
        onCloseMobile={() => setIsMobileOpen(false)}
      />
      {isMobileOpen ? <button className="sidebar-backdrop" type="button" aria-label="Cerrar menú" onClick={() => setIsMobileOpen(false)} /> : null}
      <div className="app-content">
        <Header onToggleMobile={() => setIsMobileOpen((current) => !current)} />
        <main className="main-area">
          <Breadcrumbs />
          <Outlet />
        </main>
      </div>
    </div>
  );
}

import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Breadcrumbs } from '../components/Breadcrumbs';
import { Header } from '../components/Header';
import { DataSourceIndicator } from '../components/DataSourceIndicator';
import { Sidebar } from '../components/Sidebar';

export function AppLayout() {
  const [isCollapsed, setIsCollapsed] = useState(false);
  const [isMobileOpen, setIsMobileOpen] = useState(false);

  return (
    <div className={`app-shell ${isCollapsed ? 'sidebar-collapsed' : ''}`}>
      <Sidebar
        isCollapsed={isCollapsed}
        isMobileOpen={isMobileOpen}
        onCloseMobile={() => setIsMobileOpen(false)}
      />
      {isMobileOpen ? <button className="sidebar-backdrop" type="button" aria-label="Cerrar menú" onClick={() => setIsMobileOpen(false)} /> : null}
      <div className="app-content">
        <Header
          isCollapsed={isCollapsed}
          onToggleSidebar={() => setIsCollapsed((current) => !current)}
          onToggleMobile={() => setIsMobileOpen((current) => !current)}
        />
        <main className="main-area">
          <div className="main-meta"><Breadcrumbs /><DataSourceIndicator /></div>
          <Outlet />
        </main>
      </div>
    </div>
  );
}

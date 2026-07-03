import { Link, useLocation } from 'react-router-dom';
import type { BreadcrumbItem } from '../types/navigation';

const mainRoutes = new Set([
  '/inicio',
  '/calificaciones',
  '/cargas/x-factor',
  '/cargas/x-monto',
  '/plantillas-carga',
  '/reportes',
  '/administracion/usuarios',
  '/administracion/roles-permisos',
  '/auditoria',
  '/respaldos',
]);

function buildBreadcrumbs(pathname: string): BreadcrumbItem[] {
  if (mainRoutes.has(pathname)) return [];
  const edit = pathname.match(/^\/calificaciones\/([^/]+)\/editar$/);
  if (edit) return [{ label: 'Calificaciones Tributarias', path: '/calificaciones' }, { label: 'Editar calificación' }, { label: edit[1] }];
  const copy = pathname.match(/^\/calificaciones\/([^/]+)\/copiar$/);
  if (copy) return [{ label: 'Calificaciones Tributarias', path: '/calificaciones' }, { label: 'Copiar calificación' }, { label: copy[1] }];
  if (pathname === '/calificaciones/nueva') return [{ label: 'Calificaciones Tributarias', path: '/calificaciones' }, { label: 'Nueva calificación' }];
  return [];
}

export function Breadcrumbs() {
  const { pathname } = useLocation();
  const breadcrumbs = buildBreadcrumbs(pathname);
  if (!breadcrumbs.length) return null;
  return (
    <nav className="breadcrumbs" aria-label="Ruta de navegación">
      {breadcrumbs.map((item, index) => (
        item.path ? <Link key={`${item.path}-${item.label}`} to={item.path}>{item.label}</Link> : <span key={`${item.label}-${index}`}>/ {item.label}</span>
      ))}
    </nav>
  );
}

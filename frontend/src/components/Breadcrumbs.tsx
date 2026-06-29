import { Link, useLocation } from 'react-router-dom';
import type { BreadcrumbItem } from '../types/navigation';

const labels: Record<string, string> = {
  inicio: 'Inicio',
  calificaciones: 'Calificaciones',
  nueva: 'Nueva',
  editar: 'Editar',
  cargas: 'Cargas',
  'x-factor': 'X Factor',
  'x-monto': 'X Monto',
  'plantillas-carga': 'Plantillas de carga',
  reportes: 'Reportes',
  administracion: 'Administración',
  usuarios: 'Usuarios',
  'roles-permisos': 'Roles y Permisos',
  auditoria: 'Auditoría',
  respaldos: 'Respaldos',
};

function buildBreadcrumbs(pathname: string): BreadcrumbItem[] {
  const segments = pathname.split('/').filter(Boolean);
  return segments.map((segment, index) => ({
    label: labels[segment] ?? segment,
    path: index < segments.length - 1 ? `/${segments.slice(0, index + 1).join('/')}` : undefined,
  }));
}

export function Breadcrumbs() {
  const { pathname } = useLocation();
  const breadcrumbs = buildBreadcrumbs(pathname);

  return (
    <nav className="breadcrumbs" aria-label="Ruta de navegación">
      <Link to="/inicio">Nuam Exchange</Link>
      {breadcrumbs.map((item) => (
        <span key={`${item.label}-${item.path ?? 'current'}`}>
          <span aria-hidden="true">/</span>
          {item.path ? <Link to={item.path}>{item.label}</Link> : <span>{item.label}</span>}
        </span>
      ))}
    </nav>
  );
}

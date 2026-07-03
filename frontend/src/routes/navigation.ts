import type { NavigationItem } from '../types/navigation';
import type { UserRole } from '../types/session';

export const rolePermissions: Record<UserRole, string[]> = {
  Administrador: [
    '/inicio',
    '/calificaciones',
    '/calificaciones/nueva',
    '/calificaciones/:id/editar',
    '/calificaciones/:id/copiar',
    '/cargas/x-factor',
    '/cargas/x-monto',
    '/plantillas-carga',
    '/reportes',
    '/administracion/usuarios',
    '/administracion/roles-permisos',
    '/auditoria',
    '/respaldos',
  ],
  'Analista Tributario': [
    '/inicio',
    '/calificaciones',
    '/calificaciones/nueva',
    '/calificaciones/:id/editar',
    '/calificaciones/:id/copiar',
    '/cargas/x-factor',
    '/cargas/x-monto',
    '/plantillas-carga',
    '/reportes',
  ],
  Supervisor: [
    '/inicio',
    '/calificaciones',
    '/cargas/x-factor',
    '/cargas/x-monto',
    '/plantillas-carga',
    '/reportes',
  ],
};

export const sectionMetadata = {
  '/inicio': { title: 'Inicio', description: 'Resumen consolidado de la gestión tributaria.' },
  '/calificaciones': { title: 'Calificaciones Tributarias', description: 'Consulta, creación y gestión de registros tributarios.' },
  '/calificaciones/nueva': { title: 'Nueva calificación', description: 'Ingreso controlado de una calificación tributaria.' },
  '/calificaciones/:id/editar': { title: 'Editar calificación', description: 'Modificación controlada de una calificación tributaria.' },
  '/calificaciones/:id/copiar': { title: 'Copiar calificación', description: 'Creación de una calificación a partir de un registro existente.' },
  '/cargas/x-factor': { title: 'Carga X Factor', description: 'Carga masiva y actualización controlada de factores tributarios.' },
  '/cargas/x-monto': { title: 'Carga X Monto', description: 'Carga masiva y actualización controlada de montos tributarios.' },
  '/plantillas-carga': { title: 'Plantillas de carga', description: 'Consulta y descarga de formatos para cargas masivas.' },
  '/reportes': { title: 'Reportes', description: 'Consulta y exportación de información tributaria.' },
  '/administracion/usuarios': { title: 'Usuarios', description: 'Gestión de cuentas y perfiles del sistema.' },
  '/administracion/roles-permisos': { title: 'Roles y Permisos', description: 'Consulta y administración visual de capacidades por rol.' },
  '/auditoria': { title: 'Auditoría', description: 'Consulta de trazabilidad y eventos relevantes del sistema.' },
  '/respaldos': { title: 'Respaldos', description: 'Gestión visual de respaldos del sistema.' },
  '/sin-acceso': { title: 'Acceso no autorizado', description: 'No tienes permisos para visualizar esta sección.' },
} as const;

export type SectionPath = keyof typeof sectionMetadata;

export const navigationItems: NavigationItem[] = [
  { label: 'Inicio', path: '/inicio' },
  { label: 'Calificaciones Tributarias', path: '/calificaciones' },
  { label: 'Carga X Factor', path: '/cargas/x-factor' },
  { label: 'Carga X Monto', path: '/cargas/x-monto' },
  { label: 'Plantillas de carga', path: '/plantillas-carga' },
  { label: 'Reportes', path: '/reportes' },
  { label: 'Usuarios', path: '/administracion/usuarios' },
  { label: 'Roles y Permisos', path: '/administracion/roles-permisos' },
  { label: 'Auditoría', path: '/auditoria' },
  { label: 'Respaldos', path: '/respaldos' },
];

export function isPathAllowedForRole(role: UserRole, routePath: string) {
  return rolePermissions[role].includes(routePath);
}

export function getNavigationForRole(role: UserRole) {
  return navigationItems.filter((item) => isPathAllowedForRole(role, item.path));
}

export function getSectionMetadata(pathname: string) {
  const direct = sectionMetadata[pathname as SectionPath];
  if (direct) return direct;
  if (/^\/calificaciones\/[^/]+\/editar$/.test(pathname)) return sectionMetadata['/calificaciones/:id/editar'];
  if (/^\/calificaciones\/[^/]+\/copiar$/.test(pathname)) return sectionMetadata['/calificaciones/:id/copiar'];
  return sectionMetadata['/inicio'];
}

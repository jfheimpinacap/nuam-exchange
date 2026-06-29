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

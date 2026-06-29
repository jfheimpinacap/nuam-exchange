import type { PermissionAction, PermissionCode, PermissionModule, RoleDefinition, RolePermissionMap } from '../../types/permission';
import type { UserRole } from '../../types/session';

export const permissionActions: PermissionAction[] = ['Ver', 'Crear', 'Editar', 'Eliminar', 'Procesar', 'Exportar', 'Administrar'];

export const permissionModules: PermissionModule[] = [
  { code: 'inicio', label: 'Inicio', actions: ['Ver'] },
  { code: 'calificaciones', label: 'Calificaciones', actions: ['Ver', 'Crear', 'Editar', 'Eliminar', 'Exportar'] },
  { code: 'carga-factor', label: 'Carga X Factor', actions: ['Ver', 'Procesar', 'Exportar'] },
  { code: 'carga-monto', label: 'Carga X Monto', actions: ['Ver', 'Procesar', 'Exportar'] },
  { code: 'plantillas', label: 'Plantillas de carga', actions: ['Ver', 'Exportar'] },
  { code: 'reportes', label: 'Reportes', actions: ['Ver', 'Exportar'] },
  { code: 'usuarios', label: 'Usuarios', actions: ['Ver', 'Crear', 'Editar', 'Administrar'] },
  { code: 'roles-permisos', label: 'Roles y Permisos', actions: ['Ver', 'Administrar'] },
  { code: 'auditoria', label: 'Auditoría', actions: ['Ver', 'Exportar'] },
  { code: 'respaldos', label: 'Respaldos', actions: ['Ver', 'Crear', 'Administrar'] },
];

export const roleDefinitions: RoleDefinition[] = [
  { role: 'Administrador', description: 'Control total del sistema y módulos administrativos.', status: 'Sistema' },
  { role: 'Analista Tributario', description: 'Operación tributaria, cargas, reportes y calificaciones.', status: 'Operativo' },
  { role: 'Supervisor', description: 'Consulta y exportación para supervisión operativa.', status: 'Consulta' },
];

const allPermissions = permissionModules.flatMap((module) => module.actions.map((action) => `${module.code}:${action}` as PermissionCode));
const analyst = ['inicio:Ver','calificaciones:Ver','calificaciones:Crear','calificaciones:Editar','calificaciones:Eliminar','calificaciones:Exportar','carga-factor:Ver','carga-factor:Procesar','carga-factor:Exportar','carga-monto:Ver','carga-monto:Procesar','carga-monto:Exportar','plantillas:Ver','plantillas:Exportar','reportes:Ver','reportes:Exportar'] as PermissionCode[];
const supervisor = ['inicio:Ver','calificaciones:Ver','calificaciones:Exportar','carga-factor:Ver','carga-factor:Exportar','carga-monto:Ver','carga-monto:Exportar','plantillas:Ver','plantillas:Exportar','reportes:Ver','reportes:Exportar'] as PermissionCode[];

export const defaultRolePermissions: RolePermissionMap = { Administrador: allPermissions, 'Analista Tributario': analyst, Supervisor: supervisor };
export function permissionCode(moduleCode: string, action: PermissionAction) { return `${moduleCode}:${action}` as PermissionCode; }
export function countVisibleModules(permissions: PermissionCode[]) { return permissionModules.filter((module) => permissions.includes(permissionCode(module.code, 'Ver'))).length; }
export function countUsersByRole(role: UserRole, users: { rol: UserRole }[]) { return users.filter((user) => user.rol === role).length; }
export function enforcePermissionDependencies(permissions: PermissionCode[]) { return permissions.filter((code) => code.endsWith(':Ver') || permissions.includes(`${code.split(':')[0]}:Ver` as PermissionCode)); }

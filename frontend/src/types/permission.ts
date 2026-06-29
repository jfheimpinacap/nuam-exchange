import type { UserRole } from './session';

export type PermissionAction = 'Ver' | 'Crear' | 'Editar' | 'Eliminar' | 'Procesar' | 'Exportar' | 'Administrar';
export type PermissionModuleCode = 'inicio' | 'calificaciones' | 'carga-factor' | 'carga-monto' | 'plantillas' | 'reportes' | 'usuarios' | 'roles-permisos' | 'auditoria' | 'respaldos';
export type PermissionCode = `${PermissionModuleCode}:${PermissionAction}`;

export interface PermissionModule {
  code: PermissionModuleCode;
  label: string;
  actions: PermissionAction[];
}

export type RolePermissionMap = Record<UserRole, PermissionCode[]>;

export interface RoleDefinition {
  role: UserRole;
  description: string;
  status: 'Sistema' | 'Operativo' | 'Consulta';
}

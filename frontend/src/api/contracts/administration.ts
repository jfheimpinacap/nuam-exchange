import type { PaginationRequest } from './common';
export interface UserDto { id: string; name: string; email: string; role: string; status: 'Activo' | 'Inactivo' | 'Bloqueado'; lastAccessAt?: string; }
export interface UserListRequestDto extends PaginationRequest { search?: string; role?: string; status?: string; }
export interface CreateUserRequestDto { name: string; email: string; role: string; }
export interface UpdateUserRequestDto { name: string; email: string; role: string; }
export interface ChangeUserStatusRequestDto { status: UserDto['status']; reason?: string; }
export interface ResetAccessRequestDto { notifyUser: boolean; }
export interface RoleDto { id: string; name: string; description?: string; }
export interface PermissionDto { id: string; module: string; action: string; description?: string; }
export interface RolePermissionsDto { role: string; permissions: PermissionDto[]; }
export interface UpdateRolePermissionsRequestDto { permissionIds: string[]; }

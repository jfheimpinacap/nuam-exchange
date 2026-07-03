import type { AdministrationUser, UserAccountStatus } from '../../types/administration';
import type { UserRole } from '../../types/session';

export interface AdminRoleDto { id: number; name: UserRole | string; description?: string | null; isActive: boolean; }
export interface AdminUserInput { nombre: string; email: string; rol: UserRole; estado: UserAccountStatus; cargo?: string; password?: string; }

export interface AdministrationService {
  listUsers(): Promise<AdministrationUser[]>;
  listRoles(): Promise<AdminRoleDto[]>;
  createUser(input: AdminUserInput): Promise<AdministrationUser>;
  updateUser(id: string, input: AdminUserInput): Promise<AdministrationUser>;
  resetPassword(id: string, newPassword: string): Promise<void>;
}

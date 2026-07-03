import type { HttpClient } from '../client/HttpClient';
import type { PaginatedResponse } from '../contracts/common';
import type { AdministrationUser, UserAccountStatus } from '../../types/administration';
import type { UserRole } from '../../types/session';
import type { AdministrationService, AdminRoleDto, AdminUserInput } from './AdministrationService';

interface AdminUserDto { id: number; fullName: string; email: string; jobTitle?: string | null; role: { id: number; name: string }; isActive: boolean; lastAccessAt?: string | null; createdAt: string; updatedAt: string; createdBy?: string | null; }

const toEstado = (user: AdminUserDto): UserAccountStatus => user.isActive ? 'Activo' : 'Inactivo';
const toUser = (user: AdminUserDto): AdministrationUser => ({ id: String(user.id), nombre: user.fullName, email: user.email, rol: user.role.name as UserRole, estado: toEstado(user), cargo: user.jobTitle ?? '', fechaCreacion: user.createdAt, ultimoAcceso: user.lastAccessAt ?? null, creadoPor: user.createdBy ?? 'Administración' });

export class HttpAdministrationService implements AdministrationService {
  private roles: AdminRoleDto[] = [];
  constructor(private readonly http: HttpClient) {}
  async listUsers() { const response = await this.http.get<PaginatedResponse<AdminUserDto>>('admin/users', { query: { page: 1, pageSize: 100 } }); return response.items.map(toUser); }
  async listRoles() { this.roles = await this.http.get<AdminRoleDto[]>('admin/roles'); return this.roles; }
  async createUser(input: AdminUserInput) { const roleId = await this.roleId(input.rol); const user = await this.http.post<AdminUserDto>('admin/users', { fullName: input.nombre, email: input.email, roleId, jobTitle: input.cargo || null, password: input.password, isActive: input.estado === 'Activo' }); return toUser(user); }
  async updateUser(id: string, input: AdminUserInput) { const roleId = await this.roleId(input.rol); const user = await this.http.put<AdminUserDto>(`admin/users/${id}`, { fullName: input.nombre, email: input.email, roleId, jobTitle: input.cargo || null, isActive: input.estado === 'Activo' }); return toUser(user); }
  async resetPassword(id: string, newPassword: string) { await this.http.post<void>(`admin/users/${id}/reset-password`, { newPassword }); }
  private async roleId(role: UserRole) { if (!this.roles.length) await this.listRoles(); const found = this.roles.find((item) => item.name === role); if (!found) throw new Error('Rol administrativo no disponible.'); return found.id; }
}

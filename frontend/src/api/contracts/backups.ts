import type { PaginationRequest } from './common';
export interface BackupDto { id: string; createdAt: string; createdBy: string; status: 'Pending' | 'Running' | 'Completed' | 'Failed' | 'Cancelled'; sizeBytes?: number; rowCount?: number; }
export interface BackupListRequestDto extends PaginationRequest { status?: string; from?: string; to?: string; }
export interface CreateBackupRequestDto { reason: string; includeAudit: boolean; }
export interface RestoreBackupRequestDto { reason: string; confirmationCode: string; }
export interface BackupPolicyDto { retentionDays: number; schedule: string; isEnabled: boolean; }
export interface UpdateBackupPolicyRequestDto { retentionDays: number; schedule: string; isEnabled: boolean; }

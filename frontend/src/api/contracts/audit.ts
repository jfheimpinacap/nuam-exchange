import type { PaginationRequest } from './common';
export interface AuditEventDto { id: string; occurredAt: string; userId?: string; userName: string; action: string; module: string; entityId?: string; correlationId?: string; detail?: string; }
export interface AuditListRequestDto extends PaginationRequest { from?: string; to?: string; userId?: string; module?: string; action?: string; }

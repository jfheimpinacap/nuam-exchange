import type { UserRole } from '../../types/session';
export interface UploadPermissions { canSelectFile: boolean; canValidate: boolean; canProcess: boolean; canDownload: boolean; isReviewer: boolean; }
export function getUploadPermissions(role: UserRole): UploadPermissions { const reviewer=role==='Supervisor'; return { canSelectFile: !reviewer, canValidate: !reviewer, canProcess: !reviewer, canDownload: true, isReviewer: reviewer }; }

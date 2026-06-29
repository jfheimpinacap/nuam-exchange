import type { ClassificationListRequestDto, ClassificationCatalogsDto, CreateClassificationRequestDto, UpdateClassificationRequestDto, CopyClassificationRequestDto, ClassificationWriteResponseDto } from '../contracts/classifications';
import type { PaginatedResponse } from '../contracts/common';
import type { Classification } from '../../types/classification';
export interface ClassificationsService {
  list(request: ClassificationListRequestDto, signal?: AbortSignal): Promise<PaginatedResponse<Classification>>;
  getCatalogs(signal?: AbortSignal): Promise<ClassificationCatalogsDto>;
  getById(id: string, signal?: AbortSignal): Promise<Classification>;
  create(request: CreateClassificationRequestDto, signal?: AbortSignal): Promise<ClassificationWriteResponseDto>;
  update(id: string, request: UpdateClassificationRequestDto, rowVersion?: string, signal?: AbortSignal): Promise<ClassificationWriteResponseDto>;
  copy(id: string, request: CopyClassificationRequestDto, signal?: AbortSignal): Promise<ClassificationWriteResponseDto>;
  remove(id: string, rowVersion?: string, signal?: AbortSignal): Promise<void>;
}

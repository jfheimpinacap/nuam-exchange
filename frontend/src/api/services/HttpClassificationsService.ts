import type { HttpClient } from '../client/HttpClient';
import type { ClassificationCatalogsDto, ClassificationDto, ClassificationListRequestDto, ClassificationListResponseDto, CreateClassificationRequestDto, UpdateClassificationRequestDto, CopyClassificationRequestDto, ClassificationWriteResponseDto } from '../contracts/classifications';
import { classificationDtoToDomain } from '../mappers/classificationMapper';
import type { Classification } from '../../types/classification';
import type { ClassificationsService } from './ClassificationsService';
export class HttpClassificationsService implements ClassificationsService {
  constructor(private readonly http: HttpClient) {}
  async list(request: ClassificationListRequestDto, signal?: AbortSignal) { const response = await this.http.get<ClassificationListResponseDto>('/classifications', { query: { ...request }, signal }); return { ...response, items: response.items.map(classificationDtoToDomain) }; }
  getCatalogs(signal?: AbortSignal) { return this.http.get<ClassificationCatalogsDto>('/classifications/catalogs', { signal }); }
  async getById(id: string, signal?: AbortSignal): Promise<Classification> { return classificationDtoToDomain(await this.http.get<ClassificationDto>(`/classifications/${encodeURIComponent(id)}`, { signal })); }
  create(request: CreateClassificationRequestDto, signal?: AbortSignal) { return this.http.post<ClassificationWriteResponseDto>('/classifications', request, { signal }); }
  update(id: string, request: UpdateClassificationRequestDto, rowVersion?: string, signal?: AbortSignal) { return this.http.put<ClassificationWriteResponseDto>(`/classifications/${encodeURIComponent(id)}`, request, { signal, headers: rowVersion ? { 'If-Match': rowVersion } : undefined }); }
  copy(id: string, request: CopyClassificationRequestDto, signal?: AbortSignal) { return this.http.post<ClassificationWriteResponseDto>(`/classifications/${encodeURIComponent(id)}/copy`, request, { signal }); }
  remove(id: string, rowVersion?: string, signal?: AbortSignal) { return this.http.delete<void>(`/classifications/${encodeURIComponent(id)}`, { signal, headers: rowVersion ? { 'If-Match': rowVersion } : undefined }); }
}

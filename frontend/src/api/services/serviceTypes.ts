import type { ClassificationCatalogsDto } from '../contracts/classifications';
import type { PaginatedResponse } from '../contracts/common';
import type { Classification } from '../../types/classification';
export type ClassificationCatalogs = ClassificationCatalogsDto;
export type ClassificationListResult = PaginatedResponse<Classification>;

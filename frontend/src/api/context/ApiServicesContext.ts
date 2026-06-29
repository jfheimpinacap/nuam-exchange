import { createContext } from 'react';
import type { ClassificationsService } from '../services/ClassificationsService';
export interface ApiServicesContextValue { dataSource: 'mock' | 'api'; classificationsService: ClassificationsService; apiBaseUrl: string; isMock: boolean; isApi: boolean; }
export const ApiServicesContext = createContext<ApiServicesContextValue | null>(null);

import { createContext } from 'react';
import type { ClassificationsService } from '../services/ClassificationsService';
import type { AuthService } from '../services/AuthService';

export interface ApiServicesContextValue {
  dataSource: 'mock' | 'api';
  classificationsService: ClassificationsService;
  authService: AuthService;
  apiBaseUrl: string;
  isMock: boolean;
  isApi: boolean;
}

export const ApiServicesContext = createContext<ApiServicesContextValue | null>(null);

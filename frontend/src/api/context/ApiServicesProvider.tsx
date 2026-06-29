import { createContext, useContext, type ReactNode } from 'react';
import { classificationsService, type ClassificationsService } from '../services/ClassificationsService';
interface ApiServices { classifications: ClassificationsService; }
const ApiServicesContext = createContext<ApiServices | undefined>(undefined);
export function ApiServicesProvider({ children }: { children: ReactNode }) { return <ApiServicesContext.Provider value={{ classifications: classificationsService }}>{children}</ApiServicesContext.Provider>; }
export function useApiServices() { const context = useContext(ApiServicesContext); if (!context) throw new Error('useApiServices must be used inside ApiServicesProvider'); return context; }

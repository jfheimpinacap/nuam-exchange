import { useContext } from 'react';
import { ApiServicesContext } from './ApiServicesContext';
export function useApiServices() { const context = useContext(ApiServicesContext); if (!context) throw new Error('useApiServices debe utilizarse dentro de ApiServicesProvider.'); return context; }

import type { MockUser } from '../types/session';

export const mockUsers: MockUser[] = [
  {
    id: 'admin-demo',
    nombre: 'Usuario Administrador',
    email: 'admin.demo@nuam.local',
    rol: 'Administrador',
  },
  {
    id: 'analista-demo',
    nombre: 'Usuario Analista',
    email: 'analista.demo@nuam.local',
    rol: 'Analista Tributario',
  },
  {
    id: 'supervisor-demo',
    nombre: 'Usuario Supervisor',
    email: 'supervisor.demo@nuam.local',
    rol: 'Supervisor',
  },
];

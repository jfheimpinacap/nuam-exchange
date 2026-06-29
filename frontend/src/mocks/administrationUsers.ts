import type { AdministrationUser } from '../types/administration';

export const administrationUsers: AdministrationUser[] = [
  { id: 'admin-demo', nombre: 'Usuario Administrador', email: 'admin.demo@nuam.local', rol: 'Administrador', estado: 'Activo', fechaCreacion: '2026-01-05', ultimoAcceso: '2026-06-20', creadoPor: 'Sistema' },
  { id: 'analista-demo', nombre: 'Usuario Analista', email: 'analista.demo@nuam.local', rol: 'Analista Tributario', estado: 'Activo', fechaCreacion: '2026-01-08', ultimoAcceso: '2026-06-19', creadoPor: 'Sistema' },
  { id: 'supervisor-demo', nombre: 'Usuario Supervisor', email: 'supervisor.demo@nuam.local', rol: 'Supervisor', estado: 'Activo', fechaCreacion: '2026-01-10', ultimoAcceso: '2026-06-18', creadoPor: 'Sistema' },
  { id: 'adm-ops-001', nombre: 'Operador Norte', email: 'operador.norte@nuam.local', rol: 'Administrador', estado: 'Inactivo', fechaCreacion: '2026-02-02', ultimoAcceso: null, creadoPor: 'Usuario Administrador' },
  { id: 'ana-renta-002', nombre: 'Analista Renta', email: 'analista.renta@nuam.local', rol: 'Analista Tributario', estado: 'Activo', fechaCreacion: '2026-02-12', ultimoAcceso: '2026-06-17', creadoPor: 'Usuario Administrador' },
  { id: 'sup-mercado-003', nombre: 'Supervisor Mercado', email: 'supervisor.mercado@nuam.local', rol: 'Supervisor', estado: 'Bloqueado', fechaCreacion: '2026-02-22', ultimoAcceso: '2026-05-30', creadoPor: 'Usuario Administrador' },
  { id: 'ana-custodia-004', nombre: 'Analista Custodia', email: 'analista.custodia@nuam.local', rol: 'Analista Tributario', estado: 'Inactivo', fechaCreacion: '2026-03-01', ultimoAcceso: null, creadoPor: 'Usuario Administrador' },
  { id: 'sup-control-005', nombre: 'Supervisor Control', email: 'supervisor.control@nuam.local', rol: 'Supervisor', estado: 'Activo', fechaCreacion: '2026-03-15', ultimoAcceso: '2026-06-15', creadoPor: 'Usuario Administrador' },
  { id: 'ana-pagos-006', nombre: 'Analista Pagos', email: 'analista.pagos@nuam.local', rol: 'Analista Tributario', estado: 'Activo', fechaCreacion: '2026-03-26', ultimoAcceso: '2026-06-12', creadoPor: 'Usuario Administrador' },
  { id: 'adm-soporte-007', nombre: 'Administrador Soporte', email: 'administrador.soporte@nuam.local', rol: 'Administrador', estado: 'Bloqueado', fechaCreacion: '2026-04-02', ultimoAcceso: '2026-05-22', creadoPor: 'Sistema' },
  { id: 'ana-reportes-008', nombre: 'Analista Reportes', email: 'analista.reportes@nuam.local', rol: 'Analista Tributario', estado: 'Activo', fechaCreacion: '2026-04-18', ultimoAcceso: '2026-06-21', creadoPor: 'Usuario Administrador' },
  { id: 'sup-consulta-009', nombre: 'Supervisor Consulta', email: 'supervisor.consulta@nuam.local', rol: 'Supervisor', estado: 'Inactivo', fechaCreacion: '2026-05-05', ultimoAcceso: null, creadoPor: 'Usuario Administrador' },
  { id: 'ana-factor-010', nombre: 'Analista Factor', email: 'analista.factor@nuam.local', rol: 'Analista Tributario', estado: 'Bloqueado', fechaCreacion: '2026-05-20', ultimoAcceso: '2026-06-02', creadoPor: 'Usuario Administrador' },
  { id: 'sup-cargas-011', nombre: 'Supervisor Cargas', email: 'supervisor.cargas@nuam.local', rol: 'Supervisor', estado: 'Activo', fechaCreacion: '2026-06-01', ultimoAcceso: '2026-06-22', creadoPor: 'Usuario Administrador' },
  { id: 'ana-montos-012', nombre: 'Analista Montos', email: 'analista.montos@nuam.local', rol: 'Analista Tributario', estado: 'Activo', fechaCreacion: '2026-06-10', ultimoAcceso: null, creadoPor: 'Usuario Administrador' },
];

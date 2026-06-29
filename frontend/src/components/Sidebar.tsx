import { NavLink } from 'react-router-dom';
import { useSession } from '../app/session/useSession';
import type { UserRole } from '../types';
interface LinkItem { label:string; to:string; roles:UserRole[]; }
const links: LinkItem[] = [
 {label:'Inicio',to:'/inicio',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Calificaciones',to:'/calificaciones',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Carga X Factor',to:'/cargas/x-factor',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Carga X Monto',to:'/cargas/x-monto',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Plantillas',to:'/plantillas-carga',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Reportes',to:'/reportes',roles:['Administrador','Analista Tributario','Supervisor']}, {label:'Usuarios',to:'/administracion/usuarios',roles:['Administrador']}, {label:'Roles y permisos',to:'/administracion/roles-permisos',roles:['Administrador']}, {label:'Auditoría',to:'/auditoria',roles:['Administrador']}, {label:'Respaldos',to:'/respaldos',roles:['Administrador']}
];
export function Sidebar() { const { hasRole } = useSession(); return <aside className="sidebar"><div className="brand">Nuam Exchange</div><nav>{links.filter(l=>hasRole(l.roles)).map(link=><NavLink key={link.to} to={link.to}>{link.label}</NavLink>)}</nav></aside>; }

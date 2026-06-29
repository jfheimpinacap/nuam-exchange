import { createContext, useMemo, useState, type ReactNode } from 'react';
import type { SessionUser, UserRole } from '../../types';
import type { SessionContextValue } from './sessionTypes';
export const SessionContext = createContext<SessionContextValue | undefined>(undefined);
const users: Record<UserRole, SessionUser> = { Administrador:{id:'1',name:'Administradora Demo',email:'admin.demo@nuam.local',role:'Administrador'}, 'Analista Tributario':{id:'2',name:'Analista Demo',email:'analista.demo@nuam.local',role:'Analista Tributario'}, Supervisor:{id:'3',name:'Supervisor Demo',email:'supervisor.demo@nuam.local',role:'Supervisor'} };
export function SessionProvider({ children }: { children: ReactNode }) { const [user,setUser]=useState<SessionUser|null>(null); const value=useMemo<SessionContextValue>(()=>({ user, isAuthenticated:Boolean(user), login:(role,password)=>{ if(!password.trim()) return false; setUser(users[role]); return true; }, logout:()=>setUser(null), hasRole:(roles)=>Boolean(user && roles.includes(user.role)) }),[user]); return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>; }

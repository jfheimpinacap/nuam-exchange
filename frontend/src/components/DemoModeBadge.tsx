import { isMockMode } from '../api/config';
export function DemoModeBadge() { return <span className="badge">{isMockMode ? 'Modo mock' : 'Modo API preparado'}</span>; }

import { Link } from 'react-router-dom';
export function AccessDeniedPage() { return <section className="page"><h1>Acceso denegado</h1><p>El rol activo no cuenta con permisos visuales para este módulo.</p><Link to="/inicio">Volver al inicio</Link></section>; }

import type { AdministrationUser, UserFormErrors, UserFormValues } from '../../types/administration';

const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function normalizeUserForm(values: UserFormValues): UserFormValues {
  return { ...values, nombre: values.nombre.trim(), email: values.email.trim().toLowerCase(), cargo: values.cargo?.trim() };
}

export function validateUserForm(values: UserFormValues, users: AdministrationUser[], currentId?: string): UserFormErrors {
  const normalized = normalizeUserForm(values);
  const errors: UserFormErrors = {};
  if (!normalized.nombre) errors.nombre = 'El nombre es obligatorio.';
  else if (normalized.nombre.length < 3 || normalized.nombre.length > 150) errors.nombre = 'El nombre debe tener entre 3 y 150 caracteres.';
  if (!normalized.email) errors.email = 'El correo es obligatorio.';
  else if (!emailPattern.test(normalized.email)) errors.email = 'Ingresa un correo válido.';
  else if (users.some((user) => user.email.toLowerCase() === normalized.email && user.id !== currentId)) errors.email = 'Ya existe un usuario con este correo.';
  if (!normalized.rol) errors.rol = 'Selecciona un rol.';
  if (!normalized.estado) errors.estado = 'Selecciona un estado.';
  if (normalized.password !== undefined && normalized.password.length < 8) errors.password = 'La contraseña debe tener al menos 8 caracteres y cumplir la política vigente.';
  if (normalized.password !== normalized.confirmPassword) errors.confirmPassword = 'La confirmación no coincide con la contraseña.';
  return errors;
}

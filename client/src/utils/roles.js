export const ADMIN_ROLES = [
  'SuperAdmin',
  'SchoolAdmin',
  'Principal',
  'VicePrincipal',
];

export const hasAdminRole = (roles = []) =>
  roles.some((role) => ADMIN_ROLES.includes(role));

/** ASP.NET Core Identity's default claim type URI for role claims — see ClaimTypes.Role on the backend. */
export const ROLE_CLAIM_TYPE = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

/**
 * Decodes a JWT's payload without verifying its signature — verification
 * happens on the server on every request. This is purely so the UI can
 * read claims (like role) to decide what to show; it is never a security
 * boundary on its own.
 */
export function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const payload = token.split('.')[1];
    const normalized = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = atob(normalized);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

export function getRolesFromToken(token: string): string[] {
  const payload = decodeJwtPayload(token);
  const role = payload?.[ROLE_CLAIM_TYPE];

  if (!role) {
    return [];
  }

  return Array.isArray(role) ? (role as string[]) : [role as string];
}

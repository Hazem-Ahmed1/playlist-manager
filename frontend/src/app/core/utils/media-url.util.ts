import { environment } from '../../../environments/environment';

/**
 * Turns a stored relative path (e.g. "uploads/songs/{guid}.mp3", as
 * returned by the API) into a URL the browser can actually fetch — the
 * backend serves wwwroot as static files from its origin root, not under
 * /api, so this can't just reuse environment.apiUrl.
 */
export function resolveMediaUrl(relativePath: string): string {
  const normalized = relativePath.replace(/^\/+/, '');
  return `${environment.mediaBaseUrl}/${normalized}`;
}

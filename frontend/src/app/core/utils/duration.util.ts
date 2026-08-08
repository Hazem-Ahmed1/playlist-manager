/**
 * The backend serializes TimeSpan as "hh:mm:ss" (e.g. "00:03:20"), not the
 * bare "m:ss" the UI wants to display. Naively splitting on ':' and taking
 * the first two segments silently drops the seconds for anything with an
 * hours component — this parses the full shape correctly regardless of
 * whether it includes hours.
 */
export function parseTimeSpanToSeconds(raw: string): number {
  const parts = raw.split(':').map(Number);
  if (parts.length < 2 || parts.some((n) => Number.isNaN(n))) {
    return 0;
  }

  const [hours, minutes, seconds] = parts.length === 3 ? parts : [0, parts[0], parts[1]];
  return hours * 3600 + minutes * 60 + seconds;
}

export function formatSecondsCompact(totalSeconds: number): string {
  if (!Number.isFinite(totalSeconds) || totalSeconds < 0) {
    return '0:00';
  }
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.floor(totalSeconds % 60);
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

/** Converts a backend TimeSpan string into a compact "m:ss" display string. */
export function formatDurationDisplay(raw: string): string {
  return formatSecondsCompact(parseTimeSpanToSeconds(raw));
}

/**
 * Converts a user-entered "mm:ss" into the "hh:mm:ss" shape the backend's
 * TimeSpan binder expects — "3:45" alone would otherwise parse as 3 hours
 * 45 minutes rather than 3 minutes 45 seconds.
 */
export function toTimeSpanString(mmSs: string): string | null {
  const match = /^(\d{1,2}):([0-5]?\d)$/.exec(mmSs.trim());
  if (!match) {
    return null;
  }
  const [, minutes, seconds] = match;
  return `00:${minutes.padStart(2, '0')}:${seconds.padStart(2, '0')}`;
}

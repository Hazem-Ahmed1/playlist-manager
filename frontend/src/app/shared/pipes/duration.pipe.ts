import { Pipe, PipeTransform } from '@angular/core';
import { formatDurationDisplay } from '../../core/utils/duration.util';

/** Displays a backend TimeSpan string ("00:03:20") as a compact "m:ss" ("3:20"). */
@Pipe({ name: 'duration' })
export class DurationPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return value ? formatDurationDisplay(value) : '0:00';
  }
}

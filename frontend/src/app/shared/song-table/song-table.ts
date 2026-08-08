import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Song } from '../../core/models/song.model';
import { DurationPipe } from '../pipes/duration.pipe';

@Component({
  selector: 'app-song-table',
  imports: [DurationPipe],
  templateUrl: './song-table.html',
  styleUrl: './song-table.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SongTable {
  readonly songs = input.required<Song[]>();
  readonly activeSongId = input<number | null>(null);
  readonly isLoading = input(false);

  readonly songSelected = output<Song>();

  formattedDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { SongTable } from '../../shared/song-table/song-table';
import { SongService } from '../../core/services/song.service';
import { PlayerStateService } from '../../core/services/player-state.service';
import { Song } from '../../core/models/song.model';

@Component({
  selector: 'app-songs-catalog',
  imports: [SongTable],
  templateUrl: './songs-catalog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SongsCatalog {
  private readonly songService = inject(SongService);

  readonly playerState = inject(PlayerStateService);
  readonly songs = signal<Song[]>([]);
  readonly isLoading = signal(true);

  constructor() {
    this.songService.getAll().subscribe({
      next: (songs) => {
        this.songs.set(songs);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false),
    });
  }

  onSongSelected(song: Song): void {
    this.playerState.play(song);
  }
}

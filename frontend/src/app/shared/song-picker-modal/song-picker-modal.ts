import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SongService } from '../../core/services/song.service';
import { PlaylistService } from '../../core/services/playlist.service';
import { ToastService } from '../../core/services/toast.service';
import { ApiErrorResponse } from '../../core/models/api-response.model';
import { Song } from '../../core/models/song.model';

/** Lists the full song catalog, minus songs already in the target playlist, and adds whichever one is picked. */
@Component({
  selector: 'app-song-picker-modal',
  templateUrl: './song-picker-modal.html',
  styleUrl: './song-picker-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SongPickerModal {
  private readonly songService = inject(SongService);
  private readonly playlistService = inject(PlaylistService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly playlistId = input.required<number>();
  readonly excludedSongIds = input<number[]>([]);
  readonly closed = output<void>();
  readonly songAdded = output<number>();

  readonly allSongs = signal<Song[]>([]);
  readonly isLoading = signal(true);
  readonly addingSongId = signal<number | null>(null);
  readonly searchTerm = signal('');

  readonly availableSongs = computed(() => {
    const notInPlaylist = this.allSongs().filter((song) => !this.excludedSongIds().includes(song.id));

    const term = this.searchTerm().trim().toLowerCase();
    if (!term) {
      return notInPlaylist;
    }

    return notInPlaylist.filter(
      (song) => song.title.toLowerCase().includes(term) || song.artist.toLowerCase().includes(term),
    );
  });

  constructor() {
    this.songService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (songs) => {
          this.allSongs.set(songs);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.toast.error('Failed to load the song catalog.');
        },
      });
  }

  close(): void {
    this.closed.emit();
  }

  onSearchInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.searchTerm.set(input.value);
  }

  addSong(song: Song): void {
    this.addingSongId.set(song.id);

    this.playlistService
      .addSong(this.playlistId(), song.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.addingSongId.set(null);
          this.toast.success(`"${song.title}" added to the playlist.`);
          this.songAdded.emit(song.id);
        },
        error: (error: HttpErrorResponse) => {
          this.addingSongId.set(null);
          const body = error.error as ApiErrorResponse | undefined;
          this.toast.error(body?.message ?? 'Failed to add the song.');
        },
      });
  }
}

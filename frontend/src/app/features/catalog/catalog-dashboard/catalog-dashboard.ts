import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { UploadSongModal } from '../upload-song-modal/upload-song-modal';
import { EditSongModal } from '../edit-song-modal/edit-song-modal';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { SongService } from '../../../core/services/song.service';
import { ToastService } from '../../../core/services/toast.service';
import { PlayerStateService } from '../../../core/services/player-state.service';
import { Song } from '../../../core/models/song.model';

@Component({
  selector: 'app-catalog-dashboard',
  imports: [UploadSongModal, EditSongModal, ConfirmModal],
  templateUrl: './catalog-dashboard.html',
  styleUrl: './catalog-dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CatalogDashboard {
  private readonly songService = inject(SongService);
  private readonly toast = inject(ToastService);
  private readonly playerState = inject(PlayerStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly songs = signal<Song[]>([]);
  readonly isLoading = signal(true);
  readonly showUploadModal = signal(false);
  readonly deletingSongId = signal<number | null>(null);
  readonly pendingDeleteSong = signal<Song | null>(null);
  readonly editingSong = signal<Song | null>(null);

  constructor() {
    this.loadSongs();
  }

  private loadSongs(): void {
    this.isLoading.set(true);
    this.songService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (songs) => {
          this.songs.set(songs);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.toast.error('Failed to load the catalog.');
        },
      });
  }

  onSongUploaded(song: Song): void {
    this.showUploadModal.set(false);
    this.songs.update((list) => [song, ...list]);
  }

  playSong(song: Song): void {
    this.playerState.play(song, this.songs());
  }

  deleteSong(song: Song): void {
    this.pendingDeleteSong.set(song);
  }

  confirmDeleteSong(): void {
    const song = this.pendingDeleteSong();
    this.pendingDeleteSong.set(null);
    if (!song) {
      return;
    }

    this.deletingSongId.set(song.id);
    this.songService
      .delete(song.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.deletingSongId.set(null);
          this.songs.update((list) => list.filter((s) => s.id !== song.id));
          this.toast.success('Song deleted successfully.');
        },
        error: () => {
          this.deletingSongId.set(null);
          this.toast.error('Failed to delete the song.');
        },
      });
  }

  onSongSaved(song: Song): void {
    this.editingSong.set(null);
    this.songs.update((list) => list.map((s) => (s.id === song.id ? song : s)));
  }

  onSongDeleted(songId: number): void {
    this.editingSong.set(null);
    this.songs.update((list) => list.filter((s) => s.id !== songId));
  }

  formattedDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}

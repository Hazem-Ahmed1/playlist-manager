import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { UploadSongModal } from '../upload-song-modal/upload-song-modal';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { SongService } from '../../../core/services/song.service';
import { ToastService } from '../../../core/services/toast.service';
import { Song } from '../../../core/models/song.model';

@Component({
  selector: 'app-catalog-dashboard',
  imports: [UploadSongModal, ConfirmModal],
  templateUrl: './catalog-dashboard.html',
  styleUrl: './catalog-dashboard.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CatalogDashboard {
  private readonly songService = inject(SongService);
  private readonly toast = inject(ToastService);

  readonly songs = signal<Song[]>([]);
  readonly isLoading = signal(true);
  readonly showUploadModal = signal(false);
  readonly deletingSongId = signal<number | null>(null);
  readonly pendingDeleteSong = signal<Song | null>(null);

  constructor() {
    this.loadSongs();
  }

  private loadSongs(): void {
    this.isLoading.set(true);
    this.songService.getAll().subscribe({
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
    this.songService.delete(song.id).subscribe({
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

  formattedDate(value: string): string {
    return new Date(value).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
  }
}

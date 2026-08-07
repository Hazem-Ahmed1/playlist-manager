import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { PlaylistSection } from '../../../shared/playlist-section/playlist-section';
import { PlaylistFormModal } from '../../../shared/playlist-form-modal/playlist-form-modal';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { PlaylistService } from '../../../core/services/playlist.service';
import { ToastService } from '../../../core/services/toast.service';
import { Playlist } from '../../../core/models/playlist.model';

@Component({
  selector: 'app-playlist-list',
  imports: [PlaylistSection, PlaylistFormModal, ConfirmModal],
  templateUrl: './playlist-list.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistList {
  private readonly router = inject(Router);
  private readonly playlistService = inject(PlaylistService);
  private readonly toast = inject(ToastService);

  readonly playlists = signal<Playlist[]>([]);
  readonly isLoading = signal(true);

  readonly showPlaylistForm = signal(false);
  readonly editingPlaylist = signal<Playlist | null>(null);
  readonly pendingDeletePlaylist = signal<Playlist | null>(null);

  constructor() {
    this.playlistService.getMyPlaylists().subscribe({
      next: (playlists) => {
        this.playlists.set(playlists);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.toast.error('Failed to load your playlists.');
      },
    });
  }

  onOpenPlaylist(playlist: Playlist): void {
    this.router.navigate(['/playlists', playlist.id]);
  }

  onCreatePlaylist(): void {
    this.editingPlaylist.set(null);
    this.showPlaylistForm.set(true);
  }

  onEditPlaylist(playlist: Playlist): void {
    this.editingPlaylist.set(playlist);
    this.showPlaylistForm.set(true);
  }

  onPlaylistSaved(playlist: Playlist): void {
    this.showPlaylistForm.set(false);
    const wasEditing = this.editingPlaylist() !== null;
    this.playlists.update((list) =>
      wasEditing ? list.map((p) => (p.id === playlist.id ? playlist : p)) : [playlist, ...list],
    );
  }

  onDeletePlaylist(playlist: Playlist): void {
    this.pendingDeletePlaylist.set(playlist);
  }

  confirmDeletePlaylist(): void {
    const playlist = this.pendingDeletePlaylist();
    this.pendingDeletePlaylist.set(null);
    if (!playlist) {
      return;
    }

    this.playlistService.delete(playlist.id).subscribe({
      next: () => {
        this.playlists.update((list) => list.filter((p) => p.id !== playlist.id));
        this.toast.success('Playlist deleted successfully.');
      },
      error: () => this.toast.error('Failed to delete the playlist.'),
    });
  }
}

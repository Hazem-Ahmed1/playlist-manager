import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { PlaylistSection } from '../../shared/playlist-section/playlist-section';
import { SongTable } from '../../shared/song-table/song-table';
import { PlaylistFormModal } from '../../shared/playlist-form-modal/playlist-form-modal';
import { ConfirmModal } from '../../shared/confirm-modal/confirm-modal';
import { PlaylistService } from '../../core/services/playlist.service';
import { SongService } from '../../core/services/song.service';
import { PlayerStateService } from '../../core/services/player-state.service';
import { AuthStateService } from '../../core/services/auth-state.service';
import { ModalService } from '../../core/services/modal.service';
import { ToastService } from '../../core/services/toast.service';
import { Playlist } from '../../core/models/playlist.model';
import { Song } from '../../core/models/song.model';
import { DEMO_PLAYLIST, DEMO_PLAYLIST_ID } from '../../core/constants/demo-playlist';

@Component({
  selector: 'app-home',
  imports: [PlaylistSection, SongTable, PlaylistFormModal, ConfirmModal],
  templateUrl: './home.html',
  styleUrl: './home.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Home {
  private readonly router = inject(Router);
  private readonly playlistService = inject(PlaylistService);
  private readonly songService = inject(SongService);
  private readonly modal = inject(ModalService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly authState = inject(AuthStateService);
  readonly playerState = inject(PlayerStateService);

  readonly playlists = signal<Playlist[]>([DEMO_PLAYLIST]);
  readonly songs = signal<Song[]>([]);
  readonly isLoadingSongs = signal(true);

  readonly showPlaylistForm = signal(false);
  readonly editingPlaylist = signal<Playlist | null>(null);
  readonly pendingDeletePlaylist = signal<Playlist | null>(null);

  constructor() {
    this.songService
      .getAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (songs) => {
          this.songs.set(songs);
          this.isLoadingSongs.set(false);
        },
        error: () => this.isLoadingSongs.set(false),
      });

    // Swap between the demo card and the user's real playlists the moment
    // they log in or out, without needing a page refresh.
    effect(() => {
      if (this.authState.isLoggedIn()) {
        this.loadPlaylists();
      } else {
        this.playlists.set([DEMO_PLAYLIST]);
      }
    });
  }

  private loadPlaylists(): void {
    this.playlistService
      .getMyPlaylists()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (playlists) => this.playlists.set(playlists),
        error: () => this.toast.error('Failed to load your playlists.'),
      });
  }

  onSongSelected(song: Song): void {
    this.playerState.play(song, this.songs());
  }

  onOpenPlaylist(playlist: Playlist): void {
    if (playlist.id === DEMO_PLAYLIST_ID) {
      this.modal.openLogin();
      return;
    }
    this.router.navigate(['/playlists', playlist.id]);
  }

  onCreatePlaylist(): void {
    if (!this.authState.isLoggedIn()) {
      this.modal.openLogin();
      return;
    }
    this.editingPlaylist.set(null);
    this.showPlaylistForm.set(true);
  }

  onEditPlaylist(playlist: Playlist): void {
    if (playlist.id === DEMO_PLAYLIST_ID) {
      this.modal.openLogin();
      return;
    }
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
    if (playlist.id === DEMO_PLAYLIST_ID) {
      this.modal.openLogin();
      return;
    }
    this.pendingDeletePlaylist.set(playlist);
  }

  confirmDeletePlaylist(): void {
    const playlist = this.pendingDeletePlaylist();
    this.pendingDeletePlaylist.set(null);
    if (!playlist) {
      return;
    }

    this.playlistService
      .delete(playlist.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.playlists.update((list) => list.filter((p) => p.id !== playlist.id));
          this.toast.success('Playlist deleted successfully.');
        },
        error: () => this.toast.error('Failed to delete the playlist.'),
      });
  }
}

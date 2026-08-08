import { ChangeDetectionStrategy, Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SongPickerModal } from '../../../shared/song-picker-modal/song-picker-modal';
import { PlaylistFormModal } from '../../../shared/playlist-form-modal/playlist-form-modal';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { DurationPipe } from '../../../shared/pipes/duration.pipe';
import { PlaylistService } from '../../../core/services/playlist.service';
import { ToastService } from '../../../core/services/toast.service';
import { PlayerStateService } from '../../../core/services/player-state.service';
import { PlaylistDetail as PlaylistDetailModel, PlaylistSongItem } from '../../../core/models/playlist.model';
import { PlayableSong } from '../../../core/models/song.model';
import { resolveMediaUrl } from '../../../core/utils/media-url.util';

function toPlayableSong(item: PlaylistSongItem): PlayableSong {
  return {
    id: item.songId,
    title: item.title,
    artist: item.artist,
    duration: item.duration,
    filePath: item.filePath,
  };
}

@Component({
  selector: 'app-playlist-detail',
  imports: [RouterLink, SongPickerModal, PlaylistFormModal, ConfirmModal, DurationPipe],
  templateUrl: './playlist-detail.html',
  styleUrl: './playlist-detail.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistDetail {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly playlistService = inject(PlaylistService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly playerState = inject(PlayerStateService);

  private readonly playlistId = Number(this.route.snapshot.paramMap.get('id'));

  readonly playlist = signal<PlaylistDetailModel | null>(null);
  readonly isLoading = signal(true);
  readonly showSongPicker = signal(false);
  readonly showEditForm = signal(false);
  readonly showDeleteConfirm = signal(false);
  readonly removingSongId = signal<number | null>(null);

  readonly excludedSongIds = computed(() => this.playlist()?.songs.map((s) => s.songId) ?? []);

  readonly coverUrl = computed(() => {
    const path = this.playlist()?.coverImagePath;
    return path ? resolveMediaUrl(path) : null;
  });

  constructor() {
    this.loadPlaylist();
  }

  private loadPlaylist(): void {
    this.isLoading.set(true);
    this.playlistService
      .getById(this.playlistId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (playlist) => {
          this.playlist.set(playlist);
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.toast.error('Playlist not found.');
          this.router.navigate(['/playlists']);
        },
      });
  }

  playSong(item: PlaylistSongItem): void {
    const queue = (this.playlist()?.songs ?? []).map((song) => toPlayableSong(song));
    this.playerState.play(toPlayableSong(item), queue);
  }

  removeSong(songId: number, title: string): void {
    this.removingSongId.set(songId);

    this.playlistService
      .removeSong(this.playlistId, songId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.removingSongId.set(null);
          this.playlist.update((current) =>
            current ? { ...current, songs: current.songs.filter((s) => s.songId !== songId) } : current,
          );
          this.toast.success(`"${title}" removed from the playlist.`);
        },
        error: () => {
          this.removingSongId.set(null);
          this.toast.error('Failed to remove the song.');
        },
      });
  }

  onSongAdded(): void {
    this.showSongPicker.set(false);
    this.loadPlaylist();
  }

  onPlaylistSaved(): void {
    this.showEditForm.set(false);
    this.loadPlaylist();
  }

  confirmDeletePlaylist(): void {
    this.showDeleteConfirm.set(false);

    this.playlistService
      .delete(this.playlistId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toast.success('Playlist deleted successfully.');
          this.router.navigate(['/playlists']);
        },
        error: () => this.toast.error('Failed to delete the playlist.'),
      });
  }
}

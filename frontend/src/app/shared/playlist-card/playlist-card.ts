import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Playlist } from '../../core/models/playlist.model';
import { DEMO_PLAYLIST_ID } from '../../core/constants/demo-playlist';
import { resolveMediaUrl } from '../../core/utils/media-url.util';

@Component({
  selector: 'app-playlist-card',
  templateUrl: './playlist-card.html',
  styleUrl: './playlist-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistCard {
  readonly playlist = input.required<Playlist>();

  readonly open = output<void>();
  readonly edit = output<void>();
  readonly delete = output<void>();

  readonly isDemo = computed(() => this.playlist().id === DEMO_PLAYLIST_ID);

  readonly coverUrl = computed(() => {
    const path = this.playlist().coverImagePath;
    return path ? resolveMediaUrl(path) : null;
  });

  formattedDate(): string {
    return new Date(this.playlist().createdAt).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

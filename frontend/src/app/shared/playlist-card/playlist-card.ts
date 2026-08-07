import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { NgOptimizedImage } from '@angular/common';
import { Playlist } from '../../core/models/playlist.model';
import { DEMO_PLAYLIST_ID } from '../../core/constants/demo-playlist';

@Component({
  selector: 'app-playlist-card',
  imports: [NgOptimizedImage],
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

  formattedDate(): string {
    return new Date(this.playlist().createdAt).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}

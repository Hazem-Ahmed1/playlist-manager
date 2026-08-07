import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { PlaylistCard } from '../playlist-card/playlist-card';
import { Playlist } from '../../core/models/playlist.model';

@Component({
  selector: 'app-playlist-section',
  imports: [PlaylistCard],
  templateUrl: './playlist-section.html',
  styleUrl: './playlist-section.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistSection {
  readonly playlists = input.required<Playlist[]>();

  readonly createPlaylist = output<void>();
  readonly openPlaylist = output<Playlist>();
  readonly editPlaylist = output<Playlist>();
  readonly deletePlaylist = output<Playlist>();
}

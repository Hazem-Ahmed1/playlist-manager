import { Injectable, signal } from '@angular/core';
import { PlayableSong } from '../models/song.model';

/** Which song the persistent bottom player is showing — shared across whichever page's song table (or playlist view) triggered it. */
@Injectable({ providedIn: 'root' })
export class PlayerStateService {
  private readonly currentSongSignal = signal<PlayableSong | null>(null);
  readonly currentSong = this.currentSongSignal.asReadonly();

  play(song: PlayableSong): void {
    this.currentSongSignal.set(song);
  }
}

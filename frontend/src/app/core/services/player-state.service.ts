import { Injectable, computed, signal } from '@angular/core';
import { PlayableSong } from '../models/song.model';

/**
 * Which song the persistent bottom player is showing, plus the list it was
 * played from (the "queue") so Next/Previous can step through it. Shared
 * across whichever page's song table or playlist view triggered playback.
 */
@Injectable({ providedIn: 'root' })
export class PlayerStateService {
  private readonly queueSignal = signal<PlayableSong[]>([]);
  private readonly currentSongSignal = signal<PlayableSong | null>(null);

  readonly currentSong = this.currentSongSignal.asReadonly();

  private readonly currentIndex = computed(() => {
    const current = this.currentSongSignal();
    if (!current) {
      return -1;
    }
    return this.queueSignal().findIndex((song) => song.id === current.id);
  });

  readonly hasNext = computed(() => {
    const index = this.currentIndex();
    return index >= 0 && index < this.queueSignal().length - 1;
  });

  readonly hasPrevious = computed(() => this.currentIndex() > 0);

  /**
   * Plays a song. `queue` is the full list it was chosen from (a song
   * table, a playlist's songs, etc.) — Next/Previous step through it. If
   * omitted, the song is its own one-item queue.
   */
  play(song: PlayableSong, queue: PlayableSong[] = [song]): void {
    this.queueSignal.set(queue);
    this.currentSongSignal.set(song);
  }

  next(): void {
    const index = this.currentIndex();
    const queue = this.queueSignal();
    if (index >= 0 && index < queue.length - 1) {
      this.currentSongSignal.set(queue[index + 1]);
    }
  }

  previous(): void {
    const index = this.currentIndex();
    const queue = this.queueSignal();
    if (index > 0) {
      this.currentSongSignal.set(queue[index - 1]);
    }
  }
}

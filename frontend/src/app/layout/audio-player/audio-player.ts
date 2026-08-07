import { ChangeDetectionStrategy, Component, ElementRef, effect, inject, signal, viewChild } from '@angular/core';
import { PlayerStateService } from '../../core/services/player-state.service';
import { resolveMediaUrl } from '../../core/utils/media-url.util';

function formatSeconds(totalSeconds: number): string {
  if (!Number.isFinite(totalSeconds) || totalSeconds < 0) {
    return '0:00';
  }
  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.floor(totalSeconds % 60);
  return `${minutes}:${seconds.toString().padStart(2, '0')}`;
}

@Component({
  selector: 'app-audio-player',
  templateUrl: './audio-player.html',
  styleUrl: './audio-player.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AudioPlayer {
  private readonly playerState = inject(PlayerStateService);
  private readonly audioRef = viewChild.required<ElementRef<HTMLAudioElement>>('audioEl');

  readonly song = this.playerState.currentSong;

  readonly isPlaying = signal(false);
  readonly currentTimeSeconds = signal(0);
  readonly durationSeconds = signal(0);
  readonly volume = signal(70);

  constructor() {
    // Load and auto-play the real audio file whenever a new track is picked.
    effect(() => {
      const current = this.song();
      const audio = this.audioRef().nativeElement;

      this.currentTimeSeconds.set(0);
      this.durationSeconds.set(0);

      if (!current) {
        audio.pause();
        audio.removeAttribute('src');
        return;
      }

      audio.src = resolveMediaUrl(current.filePath);
      audio.volume = this.volume() / 100;
      audio.play().catch(() => {
        // Browsers can block autoplay until the user has interacted with
        // the page at least once — isPlaying just reflects what actually happened.
        this.isPlaying.set(false);
      });
    });
  }

  progressPercent(): number {
    const duration = this.durationSeconds();
    return duration > 0 ? (this.currentTimeSeconds() / duration) * 100 : 0;
  }

  formattedProgress(): string {
    return formatSeconds(this.currentTimeSeconds());
  }

  formattedDuration(): string {
    return this.durationSeconds() > 0 ? formatSeconds(this.durationSeconds()) : (this.song()?.duration ?? '0:00');
  }

  togglePlay(): void {
    if (!this.song()) {
      return;
    }
    const audio = this.audioRef().nativeElement;
    if (audio.paused) {
      audio.play().catch(() => {});
    } else {
      audio.pause();
    }
  }

  onSeek(event: Event): void {
    const input = event.target as HTMLInputElement;
    const percent = Number(input.value);
    this.audioRef().nativeElement.currentTime = (percent / 100) * this.durationSeconds();
  }

  onVolumeChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = Number(input.value);
    this.volume.set(value);
    this.audioRef().nativeElement.volume = value / 100;
  }

  onLoadedMetadata(): void {
    this.durationSeconds.set(this.audioRef().nativeElement.duration || 0);
  }

  onTimeUpdate(): void {
    this.currentTimeSeconds.set(this.audioRef().nativeElement.currentTime);
  }

  onPlay(): void {
    this.isPlaying.set(true);
  }

  onPause(): void {
    this.isPlaying.set(false);
  }

  onEnded(): void {
    this.isPlaying.set(false);
    this.currentTimeSeconds.set(0);
  }
}

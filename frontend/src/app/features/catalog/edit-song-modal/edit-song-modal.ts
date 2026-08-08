import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, effect, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfirmModal } from '../../../shared/confirm-modal/confirm-modal';
import { SongService } from '../../../core/services/song.service';
import { ToastService } from '../../../core/services/toast.service';
import { PlayerStateService } from '../../../core/services/player-state.service';
import { ApiErrorResponse } from '../../../core/models/api-response.model';
import { Song } from '../../../core/models/song.model';
import { SONG_MESSAGES } from '../../../core/constants/validation-messages';
import { formatDurationDisplay, toTimeSpanString } from '../../../core/utils/duration.util';

/** Edit an existing catalog song's metadata, delete it, or play it — all from one modal, opened by clicking a row in the catalog table. */
@Component({
  selector: 'app-edit-song-modal',
  imports: [ReactiveFormsModule, ConfirmModal],
  templateUrl: './edit-song-modal.html',
  styleUrl: './edit-song-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditSongModal {
  private readonly fb = inject(FormBuilder);
  private readonly songService = inject(SongService);
  private readonly toast = inject(ToastService);
  private readonly playerState = inject(PlayerStateService);
  private readonly destroyRef = inject(DestroyRef);

  readonly song = input.required<Song>();
  readonly closed = output<void>();
  readonly saved = output<Song>();
  readonly deleted = output<number>();

  readonly messages = SONG_MESSAGES;
  readonly isSubmitting = signal(false);
  readonly isDeleting = signal(false);
  readonly showDeleteConfirm = signal(false);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    artist: ['', [Validators.required, Validators.maxLength(150)]],
    album: ['', [Validators.maxLength(150)]],
    genre: ['', [Validators.maxLength(100)]],
    duration: [''],
  });

  constructor() {
    effect(() => {
      const current = this.song();
      this.form.patchValue({
        title: current.title,
        artist: current.artist,
        album: current.album ?? '',
        genre: current.genre ?? '',
        duration: formatDurationDisplay(current.duration),
      });
    });
  }

  close(): void {
    this.closed.emit();
  }

  play(): void {
    const current = this.song();
    this.playerState.play({
      id: current.id,
      title: current.title,
      artist: current.artist,
      duration: current.duration,
      filePath: current.filePath,
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();

    this.songService
      .update(this.song().id, {
        title: raw.title,
        artist: raw.artist,
        album: raw.album || null,
        genre: raw.genre || null,
        duration: raw.duration ? toTimeSpanString(raw.duration) : null,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (song) => {
          this.isSubmitting.set(false);
          this.toast.success('Song updated successfully.');
          this.saved.emit(song);
        },
        error: (error: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          const body = error.error as ApiErrorResponse | undefined;
          this.toast.error(body?.errors?.[0]?.message ?? body?.message ?? 'Failed to update the song.');
        },
      });
  }

  confirmDelete(): void {
    this.showDeleteConfirm.set(false);
    this.isDeleting.set(true);

    this.songService
      .delete(this.song().id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isDeleting.set(false);
          this.toast.success('Song deleted successfully.');
          this.deleted.emit(this.song().id);
        },
        error: () => {
          this.isDeleting.set(false);
          this.toast.error('Failed to delete the song.');
        },
      });
  }
}

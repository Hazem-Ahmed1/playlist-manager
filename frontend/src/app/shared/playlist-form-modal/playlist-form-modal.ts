import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlaylistService } from '../../core/services/playlist.service';
import { ToastService } from '../../core/services/toast.service';
import { ApiErrorResponse } from '../../core/models/api-response.model';
import { Playlist, PlaylistFormSeed } from '../../core/models/playlist.model';
import { PLAYLIST_MESSAGES } from '../../core/constants/validation-messages';

/** Create/edit form for a playlist. Pass an existing Playlist via [playlist] to edit it; omit it to create a new one. */
@Component({
  selector: 'app-playlist-form-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './playlist-form-modal.html',
  styleUrl: './playlist-form-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaylistFormModal {
  private readonly fb = inject(FormBuilder);
  private readonly playlistService = inject(PlaylistService);
  private readonly toast = inject(ToastService);

  readonly playlist = input<PlaylistFormSeed | null>(null);
  readonly closed = output<void>();
  readonly saved = output<Playlist>();

  readonly messages = PLAYLIST_MESSAGES;
  readonly isSubmitting = signal(false);
  readonly isEditMode = computed(() => this.playlist() !== null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
  });

  constructor() {
    effect(() => {
      const current = this.playlist();
      this.form.setValue({
        name: current?.name ?? '',
        description: current?.description ?? '',
      });
    });
  }

  close(): void {
    this.closed.emit();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();
    const payload = { name: raw.name, description: raw.description || null };
    const current = this.playlist();

    const request$ = current
      ? this.playlistService.update(current.id, payload)
      : this.playlistService.create(payload);

    request$.subscribe({
      next: (playlist) => {
        this.isSubmitting.set(false);
        this.toast.success(current ? 'Playlist updated successfully.' : 'Playlist created successfully.');
        this.saved.emit(playlist);
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const body = error.error as ApiErrorResponse | undefined;
        this.toast.error(body?.errors?.[0]?.message ?? body?.message ?? 'Something went wrong.');
      },
    });
  }
}

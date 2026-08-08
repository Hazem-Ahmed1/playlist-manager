import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, of, switchMap } from 'rxjs';
import { PlaylistService } from '../../core/services/playlist.service';
import { ToastService } from '../../core/services/toast.service';
import { ApiErrorResponse } from '../../core/models/api-response.model';
import { Playlist, PlaylistFormSeed } from '../../core/models/playlist.model';
import { PLAYLIST_MESSAGES, COVER_MESSAGES } from '../../core/constants/validation-messages';
import { coverFileExtensionValidator, coverFileSizeValidator } from '../../core/validators/file-validators';
import { resolveMediaUrl } from '../../core/utils/media-url.util';

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
  private readonly destroyRef = inject(DestroyRef);

  readonly playlist = input<PlaylistFormSeed | null>(null);
  readonly closed = output<void>();
  readonly saved = output<Playlist>();

  readonly messages = PLAYLIST_MESSAGES;
  readonly coverMessages = COVER_MESSAGES;
  readonly isSubmitting = signal(false);
  readonly isEditMode = computed(() => this.playlist() !== null);
  readonly selectedCoverName = signal<string | null>(null);
  private readonly selectedCoverPreviewUrl = signal<string | null>(null);

  readonly existingCoverUrl = computed(() => {
    const path = this.playlist()?.coverImagePath;
    return path ? resolveMediaUrl(path) : null;
  });

  /** Whatever should actually render in the preview thumbnail: the newly-picked file if there is one, otherwise the playlist's existing cover. */
  readonly coverPreviewUrl = computed(() => this.selectedCoverPreviewUrl() ?? this.existingCoverUrl());

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
    // Cover is always optional — required is intentionally not set here.
    cover: this.fb.control<File | null>(null, [coverFileExtensionValidator, coverFileSizeValidator]),
  });

  constructor() {
    effect(() => {
      const current = this.playlist();
      this.form.patchValue({
        name: current?.name ?? '',
        description: current?.description ?? '',
      });
    });

    this.destroyRef.onDestroy(() => this.revokeSelectedCoverPreview());
  }

  close(): void {
    this.closed.emit();
  }

  onCoverSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.form.controls.cover.setValue(file);
    this.form.controls.cover.markAsTouched();
    this.selectedCoverName.set(file?.name ?? null);

    this.revokeSelectedCoverPreview();
    this.selectedCoverPreviewUrl.set(file ? URL.createObjectURL(file) : null);
  }

  private revokeSelectedCoverPreview(): void {
    const url = this.selectedCoverPreviewUrl();
    if (url) {
      URL.revokeObjectURL(url);
    }
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
    const cover = raw.cover;

    const request$ = current
      ? this.playlistService.update(current.id, payload)
      : this.playlistService.create(payload);

    request$
      .pipe(
        switchMap((playlist) => {
          if (!cover) {
            return of(playlist);
          }
          // Cover upload needs a playlist id, so it always follows
          // create/update rather than happening in the same request. Its
          // own failure is caught here, separately from a create/update
          // failure, so a cover-upload error can't be misreported as "the
          // playlist itself failed to save."
          return this.playlistService.uploadCover(playlist.id, cover).pipe(
            catchError(() => {
              this.toast.error('Playlist saved, but the cover image failed to upload.');
              return of(playlist);
            }),
          );
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
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

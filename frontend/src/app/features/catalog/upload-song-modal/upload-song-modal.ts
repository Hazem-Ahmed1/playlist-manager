import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { SongService } from '../../../core/services/song.service';
import { ToastService } from '../../../core/services/toast.service';
import { ApiErrorResponse } from '../../../core/models/api-response.model';
import { Song } from '../../../core/models/song.model';
import { ALLOWED_SONG_EXTENSIONS, SONG_MESSAGES } from '../../../core/constants/validation-messages';
import { songFileExtensionValidator, songFileSizeValidator } from '../../../core/validators/file-validators';
import { toTimeSpanString } from '../../../core/utils/duration.util';

@Component({
  selector: 'app-upload-song-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './upload-song-modal.html',
  styleUrl: './upload-song-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UploadSongModal {
  private readonly fb = inject(FormBuilder);
  private readonly songService = inject(SongService);
  private readonly toast = inject(ToastService);

  readonly closed = output<void>();
  readonly uploaded = output<Song>();

  readonly messages = SONG_MESSAGES;
  readonly allowedExtensions = ALLOWED_SONG_EXTENSIONS.join(', ');
  readonly isSubmitting = signal(false);
  readonly selectedFileName = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    artist: ['', [Validators.required, Validators.maxLength(150)]],
    album: ['', [Validators.maxLength(150)]],
    genre: ['', [Validators.maxLength(100)]],
    duration: [''],
    file: this.fb.control<File | null>(null, [
      Validators.required,
      songFileExtensionValidator,
      songFileSizeValidator,
    ]),
  });

  close(): void {
    this.closed.emit();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.form.controls.file.setValue(file);
    this.form.controls.file.markAsTouched();
    this.selectedFileName.set(file?.name ?? null);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();

    this.songService
      .upload({
        title: raw.title,
        artist: raw.artist,
        album: raw.album || null,
        genre: raw.genre || null,
        duration: raw.duration ? toTimeSpanString(raw.duration) : null,
        file: raw.file!,
      })
      .subscribe({
        next: (song) => {
          this.isSubmitting.set(false);
          this.toast.success('Song uploaded successfully.');
          this.uploaded.emit(song);
        },
        error: (error: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          const body = error.error as ApiErrorResponse | undefined;
          this.toast.error(body?.errors?.[0]?.message ?? body?.message ?? 'Upload failed.');
        },
      });
  }
}

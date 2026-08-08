import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import {
  ALLOWED_COVER_EXTENSIONS,
  ALLOWED_SONG_EXTENSIONS,
  MAX_COVER_FILE_SIZE_BYTES,
  MAX_SONG_FILE_SIZE_BYTES,
} from '../constants/validation-messages';

/** Mirrors PlaylistManagement.Api.Validation.AllowedExtensionsAttribute. */
export function fileExtensionValidator(allowedExtensions: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value as File | null;
    if (!file) {
      return null;
    }

    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
    return allowedExtensions.includes(extension) ? null : { invalidExtension: true };
  };
}

/** Mirrors PlaylistManagement.Api.Validation.MaxFileSizeAttribute. */
export function fileSizeValidator(maxBytes: number): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const file = control.value as File | null;
    if (!file) {
      return null;
    }

    return file.size <= maxBytes ? null : { fileTooLarge: true };
  };
}

// Pre-bound instances for the two file kinds the app actually uploads —
// matches PlaylistManagement.Api.DTOs.Songs.UploadSongDto and
// PlaylistManagement.Api.DTOs.Playlists.UploadCoverImageDto exactly.
export const songFileExtensionValidator = fileExtensionValidator(ALLOWED_SONG_EXTENSIONS);
export const songFileSizeValidator = fileSizeValidator(MAX_SONG_FILE_SIZE_BYTES);
export const coverFileExtensionValidator = fileExtensionValidator(ALLOWED_COVER_EXTENSIONS);
export const coverFileSizeValidator = fileSizeValidator(MAX_COVER_FILE_SIZE_BYTES);

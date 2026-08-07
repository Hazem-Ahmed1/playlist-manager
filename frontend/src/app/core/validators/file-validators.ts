import { AbstractControl, ValidationErrors } from '@angular/forms';
import { ALLOWED_SONG_EXTENSIONS, MAX_SONG_FILE_SIZE_BYTES } from '../constants/validation-messages';

/** Mirrors PlaylistManagement.Api.Validation.AllowedExtensionsAttribute as applied to UploadSongDto.File. */
export function songFileExtensionValidator(control: AbstractControl): ValidationErrors | null {
  const file = control.value as File | null;
  if (!file) {
    return null;
  }

  const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase();
  return ALLOWED_SONG_EXTENSIONS.includes(extension) ? null : { invalidExtension: true };
}

/** Mirrors PlaylistManagement.Api.Validation.MaxFileSizeAttribute as applied to UploadSongDto.File. */
export function songFileSizeValidator(control: AbstractControl): ValidationErrors | null {
  const file = control.value as File | null;
  if (!file) {
    return null;
  }

  return file.size <= MAX_SONG_FILE_SIZE_BYTES ? null : { fileTooLarge: true };
}

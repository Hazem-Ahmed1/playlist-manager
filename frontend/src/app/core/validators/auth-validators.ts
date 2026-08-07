import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Mirrors PlaylistManagement.Api.Validation.StrongPasswordAttribute: 8+
 * characters, at least one uppercase, one lowercase, one digit, one special
 * character. Unlike the backend (which reports only the first failing
 * rule), this reports every failing rule at once — friendlier for a live
 * form — but the error keys map to the exact same message text.
 */
export function strongPasswordValidator(control: AbstractControl): ValidationErrors | null {
  const value: string = control.value ?? '';

  // Emptiness is Validators.required's job; don't double-report it here.
  if (!value) {
    return null;
  }

  const errors: ValidationErrors = {};

  if (value.length < 8) {
    errors['minLength'] = true;
  }
  if (!/[A-Z]/.test(value)) {
    errors['uppercase'] = true;
  }
  if (!/[a-z]/.test(value)) {
    errors['lowercase'] = true;
  }
  if (!/[0-9]/.test(value)) {
    errors['digit'] = true;
  }
  if (!/[^a-zA-Z0-9]/.test(value)) {
    errors['specialChar'] = true;
  }

  return Object.keys(errors).length > 0 ? errors : null;
}

/** Cross-field validator for a "confirm password" field alongside a real password field. */
export function passwordsMatchValidator(passwordKey: string, confirmKey: string): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const password = group.get(passwordKey)?.value;
    const confirm = group.get(confirmKey)?.value;

    if (!confirm) {
      return null;
    }

    return password === confirm ? null : { passwordMismatch: true };
  };
}

/** Backend-matching messages for [Required]/StringLength on FirstName/LastName. */
export const NAME_MESSAGES = {
  firstNameRequired: 'First name is required.',
  firstNameLength: 'First name must be between 2 and 50 characters.',
  lastNameRequired: 'Last name is required.',
  lastNameLength: 'Last name must be between 2 and 50 characters.',
} as const;

/** Backend-matching messages for [Required]/[EmailAddress] on Email. */
export const EMAIL_MESSAGES = {
  required: 'Email is required.',
  invalid: 'Invalid email address.',
} as const;

/** Backend-matching messages, keyed the same as strongPasswordValidator's error object. */
export const PASSWORD_MESSAGES = {
  required: 'Password is required.',
  minLength: 'Password must be at least 8 characters.',
  uppercase: 'Password must contain an uppercase letter.',
  lowercase: 'Password must contain a lowercase letter.',
  digit: 'Password must contain a number.',
  specialChar: 'Password must contain a special character.',
} as const;

export const CONFIRM_PASSWORD_MESSAGES = {
  required: 'Please confirm your password.',
  mismatch: 'Passwords do not match.',
} as const;

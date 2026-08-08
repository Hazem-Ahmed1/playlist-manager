import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ModalService } from '../../../core/services/modal.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { ApiErrorResponse } from '../../../core/models/api-response.model';
import {
  CONFIRM_PASSWORD_MESSAGES,
  EMAIL_MESSAGES,
  NAME_MESSAGES,
  PASSWORD_MESSAGES,
  passwordsMatchValidator,
  strongPasswordValidator,
} from '../../../core/validators/auth-validators';

@Component({
  selector: 'app-register-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './register-modal.html',
  styleUrl: './register-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RegisterModal {
  private readonly fb = inject(FormBuilder);
  private readonly modal = inject(ModalService);
  private readonly toast = inject(ToastService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly messages = {
    name: NAME_MESSAGES,
    email: EMAIL_MESSAGES,
    password: PASSWORD_MESSAGES,
    confirmPassword: CONFIRM_PASSWORD_MESSAGES,
  };

  readonly isSubmitting = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      firstName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      lastName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, strongPasswordValidator]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatchValidator('password', 'confirmPassword') },
  );

  close(): void {
    this.modal.close();
  }

  switchToLogin(): void {
    this.modal.openLogin();
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const { confirmPassword, ...payload } = this.form.getRawValue();

    this.authService
      .register(payload)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toast.success('Registration successful.');
        this.modal.close();
        this.form.reset();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const body = error.error as ApiErrorResponse | undefined;
        const firstFieldError = body?.errors?.[0]?.message;
        this.toast.error(firstFieldError ?? body?.message ?? 'Registration failed.');
      },
    });
  }
}

import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ModalService } from '../../../core/services/modal.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { ApiErrorResponse } from '../../../core/models/api-response.model';
import { EMAIL_MESSAGES, PASSWORD_MESSAGES } from '../../../core/validators/auth-validators';

const DEMO_ADMIN = { email: 'admin@playlist.local', password: 'Admin@12345' };
const DEMO_USER = { email: 'user@playlist.local', password: 'User@12345' };

@Component({
  selector: 'app-login-modal',
  imports: [ReactiveFormsModule],
  templateUrl: './login-modal.html',
  styleUrl: './login-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginModal {
  private readonly fb = inject(FormBuilder);
  private readonly modal = inject(ModalService);
  private readonly toast = inject(ToastService);
  private readonly authService = inject(AuthService);

  readonly messages = { email: EMAIL_MESSAGES, password: PASSWORD_MESSAGES };
  readonly isSubmitting = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  close(): void {
    this.modal.close();
  }

  switchToRegister(): void {
    this.modal.openRegister();
  }

  /** Pre-fills the seeded dev accounts (see DataSeeder on the backend) so reviewers don't need to remember credentials. */
  fillDemoAccount(kind: 'admin' | 'user'): void {
    const credentials = kind === 'admin' ? DEMO_ADMIN : DEMO_USER;
    this.form.setValue(credentials);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const credentials = this.form.getRawValue();

    this.authService.login(credentials).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.toast.success('Login successful.');
        this.modal.close();
        this.form.reset();
      },
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        const body = error.error as ApiErrorResponse | undefined;
        this.toast.error(body?.message ?? 'Invalid email or password.');
      },
    });
  }
}

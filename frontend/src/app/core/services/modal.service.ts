import { Injectable, signal } from '@angular/core';

export type AuthModal = 'none' | 'login' | 'register';

/** Tracks which auth modal (if any) is open, so Sidebar can open one and either modal can switch to the other. */
@Injectable({ providedIn: 'root' })
export class ModalService {
  private readonly activeModalSignal = signal<AuthModal>('none');
  readonly activeModal = this.activeModalSignal.asReadonly();

  openLogin(): void {
    this.activeModalSignal.set('login');
  }

  openRegister(): void {
    this.activeModalSignal.set('register');
  }

  close(): void {
    this.activeModalSignal.set('none');
  }
}

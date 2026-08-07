import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthStateService } from '../../core/services/auth-state.service';
import { AuthService } from '../../core/services/auth.service';
import { ModalService } from '../../core/services/modal.service';
import { ToastService } from '../../core/services/toast.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Sidebar {
  private readonly authState = inject(AuthStateService);
  private readonly authService = inject(AuthService);
  private readonly modal = inject(ModalService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly isLoggedIn = this.authState.isLoggedIn;
  readonly isAdmin = this.authState.isAdmin;
  readonly currentUser = this.authState.currentUser;

  openLogin(): void {
    this.modal.openLogin();
  }

  openRegister(): void {
    this.modal.openRegister();
  }

  logout(): void {
    this.authService.logout();
    this.toast.info('You have been logged out.');
    this.router.navigate(['/']);
  }
}

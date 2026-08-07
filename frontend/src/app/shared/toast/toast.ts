import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ToastService } from '../../core/services/toast.service';
import { ToastType } from '../../core/models/toast.model';

const ICONS: Record<ToastType, string> = {
  success: 'fa-circle-check',
  error: 'fa-circle-xmark',
  warning: 'fa-triangle-exclamation',
  info: 'fa-circle-info',
};

@Component({
  selector: 'app-toast-container',
  templateUrl: './toast.html',
  styleUrl: './toast.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ToastContainer {
  private readonly toastService = inject(ToastService);

  readonly toasts = this.toastService.toasts;

  iconFor(type: ToastType): string {
    return ICONS[type];
  }

  dismiss(id: number): void {
    this.toastService.dismiss(id);
  }
}

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

/** Generic confirm/cancel modal — replaces native confirm() so destructive actions get a themed, accessible dialog instead of a browser prompt. */
@Component({
  selector: 'app-confirm-modal',
  templateUrl: './confirm-modal.html',
  styleUrl: './confirm-modal.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ConfirmModal {
  readonly title = input('Are you sure?');
  readonly message = input.required<string>();
  readonly confirmLabel = input('Delete');
  readonly cancelLabel = input('Cancel');
  readonly danger = input(true);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
}

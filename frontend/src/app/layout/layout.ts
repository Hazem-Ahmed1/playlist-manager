import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Sidebar } from './sidebar/sidebar';
import { AudioPlayer } from './audio-player/audio-player';
import { LoginModal } from '../features/auth/login-modal/login-modal';
import { RegisterModal } from '../features/auth/register-modal/register-modal';
import { ToastContainer } from '../shared/toast/toast';
import { ModalService } from '../core/services/modal.service';

@Component({
  selector: 'app-layout',
  imports: [RouterOutlet, Sidebar, AudioPlayer, LoginModal, RegisterModal, ToastContainer],
  templateUrl: './layout.html',
  styleUrl: './layout.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Layout {
  readonly modal = inject(ModalService);
}

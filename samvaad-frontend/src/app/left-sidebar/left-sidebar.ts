import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { NotificationService } from '../core/services/notification.service';

@Component({
  selector: 'app-left-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './left-sidebar.html'
})
export class LeftSidebar {
  auth = inject(AuthService);
  notifService = inject(NotificationService);
}

import { Component, signal, HostListener, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/services/auth.service';
import { NotificationService } from '../core/services/notification.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html'
})
export class Navbar implements OnInit, OnDestroy {
  dropdownOpen = signal(false);
  protected auth = inject(AuthService);
  protected notifService = inject(NotificationService);
  private pollTimer: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.notifService.refreshUnreadCount();
    this.pollTimer = setInterval(() => this.notifService.refreshUnreadCount(), 60_000);
  }

  ngOnDestroy(): void {
    if (this.pollTimer) clearInterval(this.pollTimer);
  }

  @HostListener('document:click')
  closeDropdown() {
    this.dropdownOpen.set(false);
  }

  logout(): void {
    this.dropdownOpen.set(false);
    this.auth.logout();
  }
}

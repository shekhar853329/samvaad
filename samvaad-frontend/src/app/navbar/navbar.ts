import { Component, signal, HostListener, inject, OnInit, OnDestroy } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../core/services/auth.service';
import { NotificationService } from '../core/services/notification.service';
import { MessageService } from '../core/services/message.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, FormsModule],
  templateUrl: './navbar.html'
})
export class Navbar implements OnInit, OnDestroy {
  dropdownOpen = signal(false);
  navSearchQuery = '';
  protected auth = inject(AuthService);
  protected notifService = inject(NotificationService);
  protected msgService = inject(MessageService);
  private router = inject(Router);
  private pollTimer: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.notifService.refreshUnreadCount();
    this.msgService.refreshUnreadCount();
    this.pollTimer = setInterval(() => {
      this.notifService.refreshUnreadCount();
      this.msgService.refreshUnreadCount();
    }, 60_000);
  }

  ngOnDestroy(): void {
    if (this.pollTimer) clearInterval(this.pollTimer);
  }

  @HostListener('document:click')
  closeDropdown() {
    this.dropdownOpen.set(false);
  }

  performSearch(): void {
    const q = this.navSearchQuery.trim();
    if (!q) return;
    this.router.navigate(['/explore'], { queryParams: { q } });
    this.navSearchQuery = '';
  }

  logout(): void {
    this.dropdownOpen.set(false);
    this.auth.logout();
  }

  get userInitials(): string {
    const name = this.auth.user()?.displayName ?? '';
    return name.split(' ').filter(w => w).map(w => w[0]).join('').toUpperCase().slice(0, 2);
  }
}

import { Component, signal, HostListener, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html'
})
export class Navbar {
  dropdownOpen = signal(false);
  protected auth = inject(AuthService);

  @HostListener('document:click')
  closeDropdown() {
    this.dropdownOpen.set(false);
  }

  logout(): void {
    this.dropdownOpen.set(false);
    this.auth.logout();
  }
}

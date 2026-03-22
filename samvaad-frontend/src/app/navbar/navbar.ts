import { Component, signal, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.html'
})
export class Navbar {
  dropdownOpen = signal(false);

  @HostListener('document:click')
  closeDropdown() {
    this.dropdownOpen.set(false);
  }
}

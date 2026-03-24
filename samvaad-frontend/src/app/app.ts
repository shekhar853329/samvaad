import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './navbar/navbar';
import { LeftSidebar } from './left-sidebar/left-sidebar';
import { RightSidebar } from './right-sidebar/right-sidebar';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, LeftSidebar, RightSidebar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected auth = inject(AuthService);
}

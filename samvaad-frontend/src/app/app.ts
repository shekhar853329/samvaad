import { Component, inject, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './navbar/navbar';
import { LeftSidebar } from './left-sidebar/left-sidebar';
import { RightSidebar } from './right-sidebar/right-sidebar';
import { AuthService } from './core/services/auth.service';
import { ChatHubService } from './core/services/chat-hub.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, LeftSidebar, RightSidebar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected auth = inject(AuthService);
  private chatHub = inject(ChatHubService);

  constructor() {
    effect(() => {
      if (this.auth.isLoggedIn()) {
        this.chatHub.connect();
      } else {
        this.chatHub.disconnect();
      }
    });
  }
}

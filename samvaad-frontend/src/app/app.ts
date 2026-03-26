import { Component, inject, effect } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Navbar } from './navbar/navbar';
import { LeftSidebar } from './left-sidebar/left-sidebar';
import { RightSidebar } from './right-sidebar/right-sidebar';
import { FloatingChat } from './floating-chat/floating-chat';
import { AuthService } from './core/services/auth.service';
import { ChatHubService } from './core/services/chat-hub.service';
import { ThemeService } from './core/services/theme.service';
import { FontService } from './core/services/font.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, LeftSidebar, RightSidebar, FloatingChat],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected auth = inject(AuthService);
  private chatHub = inject(ChatHubService);
  // Injecting ThemeService and FontService ensures they initialize (applying stored prefs) at app startup.
  private theme = inject(ThemeService);
  private font = inject(FontService);

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

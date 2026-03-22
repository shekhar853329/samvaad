import { Component, signal } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-messages',
  imports: [NgClass],
  templateUrl: './messages.html'
})
export class Messages {
  activeThread = signal<string | null>('priya');
  activeTab = signal('all');

  showChat(id: string) {
    this.activeThread.set(id);
  }

  backToList() {
    this.activeThread.set(null);
  }
}

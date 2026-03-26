import {
  Component, inject, signal, ViewChild, ElementRef,
  effect, ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FloatingChatService } from '../core/services/floating-chat.service';

@Component({
  selector: 'app-floating-chat',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './floating-chat.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styles: [`
    .fc-mine   { background: var(--color-accent); color: #fff; }
    .fc-theirs { background: #F0EFF8; color: #18162e; }
  `]
})
export class FloatingChat {
  readonly svc = inject(FloatingChatService);
  readonly inputText = signal('');

  @ViewChild('messagesEnd') private messagesEnd!: ElementRef;
  @ViewChild('messagesContainer') private messagesContainer!: ElementRef;

  constructor() {
    // Scroll to bottom whenever the message list grows or the chat opens
    effect(() => {
      const count = this.svc.messages().length;
      const minimized = this.svc.isMinimized();
      if (count > 0 && !minimized) {
        // Defer until after the DOM has rendered the new message
        setTimeout(() => {
          const el = this.messagesContainer?.nativeElement as HTMLElement | undefined;
          if (el) el.scrollTop = el.scrollHeight;
        }, 0);
      }
    });
  }

  send(): void {
    const text = this.inputText().trim();
    if (!text) return;
    this.svc.sendMessage(text);
    this.inputText.set('');
  }

  onKey(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  initials(name: string): string {
    return name.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2);
  }
}

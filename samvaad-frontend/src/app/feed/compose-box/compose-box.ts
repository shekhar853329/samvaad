import { Component, computed, inject, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { PostService } from '../../core/services/post.service';
import { Post } from '../../core/models/post.models';

@Component({
  selector: 'app-compose-box',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './compose-box.html',
})
export class ComposeBox {
  private postService = inject(PostService);
  private auth = inject(AuthService);

  /** Emitted when a new post has been created */
  postCreated = output<Post>();

  composing = signal(false);
  content = '';
  posting = signal(false);

  avatarUrl = computed(() => this.auth.user()?.avatarUrl ?? null);
  displayName = computed(() => this.auth.user()?.displayName ?? '');

  initials = computed(() => {
    const name = this.displayName();
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || 'U';
  });

  greetingName(): string {
    const name = this.auth.user()?.displayName;
    if (!name) return 'there';
    return name.split(' ')[0] ?? 'there';
  }

  cancel(): void {
    this.composing.set(false);
    this.content = '';
  }

  submit(): void {
    const text = this.content.trim();
    if (!text || this.posting()) return;
    this.posting.set(true);
    this.postService.createPost({ content: text }).subscribe({
      next: newPost => {
        this.postCreated.emit(newPost);
        this.content = '';
        this.composing.set(false);
        this.posting.set(false);
      },
      error: () => this.posting.set(false),
    });
  }
}

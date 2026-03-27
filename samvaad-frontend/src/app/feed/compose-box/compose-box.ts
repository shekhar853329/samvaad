import {
  Component,
  computed,
  inject,
  OnDestroy,
  output,
  signal,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { PostService } from '../../core/services/post.service';
import { Post } from '../../core/models/post.models';

export interface MediaAttachment {
  file: File;
  previewUrl: string;
  type: 'image' | 'video' | 'audio';
}

@Component({
  selector: 'app-compose-box',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './compose-box.html',
})
export class ComposeBox implements OnDestroy {
  private postService = inject(PostService);
  private auth = inject(AuthService);

  @ViewChild('photoInput') photoInput!: ElementRef<HTMLInputElement>;
  @ViewChild('videoInput') videoInput!: ElementRef<HTMLInputElement>;
  @ViewChild('audioInput') audioInput!: ElementRef<HTMLInputElement>;
  @ViewChild('textArea') textArea!: ElementRef<HTMLTextAreaElement>;

  /** Emitted when a new post has been created */
  postCreated = output<Post>();

  composing = signal(false);
  content = '';
  posting = signal(false);
  dragOver = signal(false);
  attachments = signal<MediaAttachment[]>([]);
  errorMsg = signal<string | null>(null);

  readonly MAX_CHARS = 500;
  readonly MAX_FILES = 6;

  avatarUrl = computed(() => this.auth.user()?.avatarUrl ?? null);
  displayName = computed(() => this.auth.user()?.displayName ?? '');

  initials = computed(() => {
    const name = this.displayName();
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2) || 'U';
  });

  charCount = computed(() => this.content.length);
  charPercent = computed(() => Math.min((this.charCount() / this.MAX_CHARS) * 100, 100));
  charNearLimit = computed(() => this.charCount() >= this.MAX_CHARS * 0.8);
  charAtLimit = computed(() => this.charCount() >= this.MAX_CHARS);

  greetingName(): string {
    const name = this.auth.user()?.displayName;
    if (!name) return 'there';
    return name.split(' ')[0] ?? 'there';
  }

  openComposer(): void {
    this.composing.set(true);
    this.errorMsg.set(null);
    setTimeout(() => this.textArea?.nativeElement.focus(), 50);
  }

  cancel(): void {
    this.composing.set(false);
    this.content = '';
    this.errorMsg.set(null);
    this.revokeAll();
    this.attachments.set([]);
  }

  submit(): void {
    const text = this.content.trim();
    if ((!text && this.attachments().length === 0) || this.posting() || this.charAtLimit()) return;
    this.errorMsg.set(null);
    this.posting.set(true);
    const mediaFiles = this.attachments().map(a => a.file);
    this.postService.createPost({ content: text, mediaFiles }).subscribe({
      next: newPost => {
        this.postCreated.emit(newPost);
        this.content = '';
        this.revokeAll();
        this.attachments.set([]);
        this.composing.set(false);
        this.posting.set(false);
      },
      error: (err) => {
        this.posting.set(false);
        this.errorMsg.set(err?.error?.message || 'Failed to create post. Please try again.');
        console.error('Post creation failed', err);
      },
    });
  }

  // ─── File pickers ────────────────────────────────────────────────────────────

  pickPhotos(): void { this.photoInput.nativeElement.click(); }
  pickVideos(): void { this.videoInput.nativeElement.click(); }
  pickAudio(): void { this.audioInput.nativeElement.click(); }

  onFileSelected(event: Event, type: 'image' | 'video' | 'audio'): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    this.addFiles(Array.from(input.files), type);
    input.value = ''; // allow re-selecting the same file
  }

  // ─── Drag & Drop ─────────────────────────────────────────────────────────────

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(true);
  }

  onDragLeave(): void {
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
    if (!event.dataTransfer?.files) return;
    const files = Array.from(event.dataTransfer.files);
    files.forEach(f => {
      const type = this.detectType(f);
      if (type) this.addFiles([f], type);
    });
  }

  // ─── Attachment management ────────────────────────────────────────────────────

  removeAttachment(index: number): void {
    const current = this.attachments();
    URL.revokeObjectURL(current[index].previewUrl);
    this.attachments.set(current.filter((_, i) => i !== index));
  }

  private addFiles(files: File[], type: 'image' | 'video' | 'audio'): void {
    const remaining = this.MAX_FILES - this.attachments().length;
    const toAdd = files.slice(0, remaining).map(f => ({
      file: f,
      previewUrl: URL.createObjectURL(f),
      type,
    }));
    this.attachments.update(prev => [...prev, ...toAdd]);
  }

  private detectType(f: File): 'image' | 'video' | 'audio' | null {
    if (f.type.startsWith('image/')) return 'image';
    if (f.type.startsWith('video/')) return 'video';
    if (f.type.startsWith('audio/')) return 'audio';
    return null;
  }

  private revokeAll(): void {
    this.attachments().forEach(a => URL.revokeObjectURL(a.previewUrl));
  }

  ngOnDestroy(): void {
    this.revokeAll();
  }

  // ─── Auto-grow textarea ───────────────────────────────────────────────────────
  autoGrow(el: HTMLTextAreaElement): void {
    el.style.height = 'auto';
    el.style.height = el.scrollHeight + 'px';
  }
}

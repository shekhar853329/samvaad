import { Component, input, output } from '@angular/core';
import { Post } from '../../core/models/post.models';
import { PostCard } from '../../shared/post-card/post-card';

@Component({
  selector: 'app-posts-list',
  standalone: true,
  imports: [PostCard],
  templateUrl: './posts-list.html',
})
export class PostsList {
  posts = input.required<Post[]>();
  loading = input.required<boolean>();
  hasMore = input.required<boolean>();

  postUpdated = output<Post>();
  postDeleted = output<string>();
  loadMore = output<void>();
}

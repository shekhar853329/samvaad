import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ExploreService } from '../core/services/explore.service';
import { TrendingHashtag } from '../core/models/explore.models';

@Component({
  selector: 'app-trending',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './trending.html'
})
export class Trending implements OnInit {
  private exploreService = inject(ExploreService);
  private router = inject(Router);

  hashtags = signal<TrendingHashtag[]>([]);

  ngOnInit(): void {
    this.exploreService.getTrendingHashtags(5).subscribe({
      next: tags => this.hashtags.set(tags)
    });
  }

  navigateToHashtag(tag: string): void {
    this.router.navigate(['/explore'], { queryParams: { q: tag } });
  }

  formatCount(n: number): string {
    if (n >= 1000) return (n / 1000).toFixed(1).replace(/\.0$/, '') + 'k';
    return n > 0 ? n.toString() : '';
  }
}

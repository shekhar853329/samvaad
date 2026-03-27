import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { SocietyService } from '../core/services/society.service';
import { AuthService } from '../core/services/auth.service';
import { Society, CreateSocietyRequest } from '../core/models/society.models';

@Component({
  selector: 'app-societies',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './societies.html',
  styleUrl: './societies.css',
})
export class Societies implements OnInit {
  private societyService = inject(SocietyService);
  private router = inject(Router);
  auth = inject(AuthService);

  societies = signal<Society[]>([]);
  loading = signal(true);
  hasMore = signal(false);
  currentPage = 1;
  readonly pageSize = 20;

  showCreateModal = signal(false);
  creating = signal(false);
  createForm: CreateSocietyRequest = { name: '', description: '' };
  createError = signal('');

  activeTab = signal<'all' | 'mine'>('all');

  ngOnInit(): void {
    this.loadSocieties();
  }

  loadSocieties(page = 1): void {
    this.loading.set(true);
    const obs = this.activeTab() === 'mine'
      ? this.societyService.getMine(page, this.pageSize)
      : this.societyService.getAll(page, this.pageSize);

    obs.subscribe({
      next: result => {
        if (page === 1) {
          this.societies.set(result.societies);
        } else {
          this.societies.update(prev => [...prev, ...result.societies]);
        }
        this.hasMore.set(result.hasMore);
        this.currentPage = page;
        this.loading.set(false);
      },
      error: () => this.loading.set(false)
    });
  }

  switchTab(tab: 'all' | 'mine'): void {
    if (this.activeTab() === tab) return;
    this.activeTab.set(tab);
    this.currentPage = 1;
    this.loadSocieties(1);
  }

  loadMore(): void {
    if (!this.hasMore() || this.loading()) return;
    this.loadSocieties(this.currentPage + 1);
  }

  navigateTo(id: string): void {
    this.router.navigate(['/societies', id]);
  }

  openCreateModal(): void {
    this.createForm = { name: '', description: '' };
    this.createError.set('');
    this.showCreateModal.set(true);
  }

  closeCreateModal(): void {
    this.showCreateModal.set(false);
  }

  submitCreate(): void {
    const name = this.createForm.name.trim();
    if (!name || this.creating()) return;
    this.creating.set(true);
    this.createError.set('');

    this.societyService.create({ name, description: this.createForm.description?.trim() }).subscribe({
      next: society => {
        this.societies.update(prev => [society, ...prev]);
        this.creating.set(false);
        this.showCreateModal.set(false);
        this.router.navigate(['/societies', society.id]);
      },
      error: (err) => {
        this.createError.set(err?.error?.message ?? 'Failed to create society.');
        this.creating.set(false);
      }
    });
  }

  joinSociety(society: Society, event: MouseEvent): void {
    event.stopPropagation();
    this.societyService.join(society.id).subscribe({
      next: updated => {
        this.societies.update(prev => prev.map(s => s.id === updated.id ? updated : s));
      }
    });
  }

  getInitials(name: string): string {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
  }
}

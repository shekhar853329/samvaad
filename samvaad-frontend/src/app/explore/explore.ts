import { Component, signal } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-explore',
  imports: [NgClass],
  templateUrl: './explore.html'
})
export class Explore {
  activeFilter = signal('all');
  following = signal(new Set<string>(['dev.m']));
  rsvped = signal(new Set<string>(['angular-india']));

  setFilter(filter: string) {
    this.activeFilter.set(filter);
  }

  toggleFollow(id: string) {
    this.following.update(s => {
      const next = new Set(s);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  toggleRsvp(id: string) {
    this.rsvped.update(s => {
      const next = new Set(s);
      next.has(id) ? next.delete(id) : next.add(id);
      return next;
    });
  }

  isFollowing(id: string) {
    return this.following().has(id);
  }

  isRsvped(id: string) {
    return this.rsvped().has(id);
  }
}

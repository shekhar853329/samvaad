import { Component } from '@angular/core';
import { OnlineFriends } from '../online-friends/online-friends';
import { Trending } from '../trending/trending';
import { WhoToFollow } from '../who-to-follow/who-to-follow';

@Component({
  selector: 'app-right-sidebar',
  imports: [OnlineFriends, Trending, WhoToFollow],
  templateUrl: './right-sidebar.html'
})
export class RightSidebar {}

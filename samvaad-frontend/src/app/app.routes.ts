import { Routes } from '@angular/router';
import { Feed } from './feed/feed';
import { Profile } from './profile/profile';
import { Notifications } from './notifications/notifications';
import { Settings } from './settings/settings';
import { Messages } from './messages/messages';
import { Explore } from './explore/explore';
import { Login } from './auth/login/login';
import { Register } from './auth/register/register';
import { FriendsPage } from './friends-page/friends-page';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Guest-only routes
  { path: 'login', component: Login, canActivate: [guestGuard] },
  { path: 'register', component: Register, canActivate: [guestGuard] },

  // Protected routes
  { path: '', component: Feed, canActivate: [authGuard] },
  { path: 'users/:username', component: Profile, canActivate: [authGuard] },
  { path: 'users/:username/friends', component: FriendsPage, canActivate: [authGuard] },
  { path: 'notifications', component: Notifications, canActivate: [authGuard] },
  { path: 'settings', component: Settings, canActivate: [authGuard] },
  { path: 'messages', component: Messages, canActivate: [authGuard] },
  { path: 'explore', component: Explore, canActivate: [authGuard] },

  { path: '**', redirectTo: '' }
];

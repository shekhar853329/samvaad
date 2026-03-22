import { Routes } from '@angular/router';
import { Feed } from './feed/feed';
import { Profile } from './profile/profile';
import { Notifications } from './notifications/notifications';
import { Settings } from './settings/settings';
import { Messages } from './messages/messages';

export const routes: Routes = [
  { path: '', component: Feed },
  { path: 'profile', component: Profile },
  { path: 'notifications', component: Notifications },
  { path: 'settings', component: Settings },
  { path: 'messages', component: Messages },
  { path: '**', redirectTo: '' }
];

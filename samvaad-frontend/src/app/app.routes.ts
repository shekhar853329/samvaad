import { Routes } from '@angular/router';
import { Feed } from './feed/feed';
import { Profile } from './profile/profile';
import { Notifications } from './notifications/notifications';

export const routes: Routes = [
  { path: '', component: Feed },
  { path: 'profile', component: Profile },
  { path: 'notifications', component: Notifications },
  { path: '**', redirectTo: '' }
];

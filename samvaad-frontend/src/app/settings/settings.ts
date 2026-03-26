import { Component, signal, OnInit, inject } from '@angular/core';
import { NgClass } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { SettingsProfile } from './profile/profile';
import { SettingsAccount } from './account/account';
import { SettingsSecurity } from './security/security';
import { SettingsAppearance } from './appearance/appearance';
import { SettingsNotifications } from './notifications/notifications';
import { SettingsPrivacy } from './privacy/privacy';
import { SettingsFeed } from './feed/feed';
import { SettingsLanguage } from './language/language';
import { SettingsIntegrations } from './integrations/integrations';
import { SettingsData } from './data/data';
import { SettingsDanger } from './danger/danger';

@Component({
  selector: 'app-settings',
  imports: [
    NgClass,
    SettingsProfile,
    SettingsAccount,
    SettingsSecurity,
    SettingsAppearance,
    SettingsNotifications,
    SettingsPrivacy,
    SettingsFeed,
    SettingsLanguage,
    SettingsIntegrations,
    SettingsData,
    SettingsDanger
  ],
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {
  private route = inject(ActivatedRoute);

  activeSection = signal('profile');

  ngOnInit(): void {
    const section = this.route.snapshot.queryParamMap.get('section');
    if (section) this.activeSection.set(section);
  }

  showSection(id: string) {
    this.activeSection.set(id);
  }
}

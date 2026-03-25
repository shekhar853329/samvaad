import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-settings-integrations',
  templateUrl: './integrations.html'
})
export class SettingsIntegrations {
  toast = signal('');

  notify(msg: string): void {
    this.toast.set(msg);
    setTimeout(() => this.toast.set(''), 3500);
  }
}

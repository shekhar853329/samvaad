import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-settings-language',
  imports: [FormsModule],
  templateUrl: './language.html'
})
export class SettingsLanguage implements OnInit {
  displayLanguage = signal('English (US)');
  contentLanguages = signal('English + Hindi');
  timezone = signal('Asia/Kolkata (IST, UTC+5:30)');
  dateFormat = signal('DD/MM/YYYY');
  saved = signal(false);

  ngOnInit(): void {
    this.displayLanguage.set(localStorage.getItem('pref:displayLanguage') ?? 'English (US)');
    this.contentLanguages.set(localStorage.getItem('pref:contentLanguages') ?? 'English + Hindi');
    this.timezone.set(localStorage.getItem('pref:timezone') ?? 'Asia/Kolkata (IST, UTC+5:30)');
    this.dateFormat.set(localStorage.getItem('pref:dateFormat') ?? 'DD/MM/YYYY');
  }

  save(): void {
    localStorage.setItem('pref:displayLanguage', this.displayLanguage());
    localStorage.setItem('pref:contentLanguages', this.contentLanguages());
    localStorage.setItem('pref:timezone', this.timezone());
    localStorage.setItem('pref:dateFormat', this.dateFormat());
    this.saved.set(true);
    setTimeout(() => this.saved.set(false), 3000);
  }
}

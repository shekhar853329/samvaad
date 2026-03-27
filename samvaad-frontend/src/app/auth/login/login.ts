import { Component, inject, signal, AfterViewInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { environment } from '../../../environments/environment';

declare var google: any;

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login implements AfterViewInit {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    identifier: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.error.set(null);
    this.loading.set(true);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Invalid credentials.');
        this.loading.set(false);
      },
    });
  }

  ngAfterViewInit(): void {
    this.initGoogleAuth();
  }

  private initGoogleAuth(): void {
    if (typeof google === 'undefined' || !google.accounts) {
      setTimeout(() => this.initGoogleAuth(), 100);
      return;
    }

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (resp: any) => this.handleGoogleLogin(resp)
    });

    const googleBtn = document.getElementById('google-btn');
    if (googleBtn) {
      google.accounts.id.renderButton(
        googleBtn,
        { theme: 'outline', size: 'large', type: 'standard', width: 350 }
      );
    }
  }

  private handleGoogleLogin(response: any): void {
    if (response.credential) {
      this.loading.set(true);
      this.error.set(null);
      this.auth.loginWithGoogle({ credential: response.credential }).subscribe({
        next: () => this.router.navigate(['/']),
        error: (err) => {
          this.error.set(err?.error?.detail ?? err?.error?.title ?? 'Google sign-in failed.');
          this.loading.set(false);
        },
      });
    }
  }
}

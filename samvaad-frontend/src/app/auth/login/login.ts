import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html',
})
export class Login {
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
}

import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html',
})
export class Register {
  private fb = inject(FormBuilder);
  private auth = inject(AuthService);
  private router = inject(Router);

  form = this.fb.nonNullable.group({
    username: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(50), Validators.pattern(/^[a-z0-9._]+$/)]],
    displayName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (this.form.invalid || this.loading()) return;

    this.error.set(null);
    this.loading.set(true);

    this.auth.register(this.form.getRawValue()).subscribe({
      next: () => this.router.navigate(['/']),
      error: (err) => {
        const detail = err?.error?.errors
          ? Object.values(err.error.errors).flat().join(' ')
          : (err?.error?.detail ?? err?.error?.title ?? 'Registration failed. Please try again.');
        this.error.set(detail as string);
        this.loading.set(false);
      },
    });
  }
}

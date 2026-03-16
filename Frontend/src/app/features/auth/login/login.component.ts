import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-card__header">
          <h1>📅 EventCalendar</h1>
          <p>Sign in to your account</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label>Username or Email</label>
            <input formControlName="usernameOrEmail" type="text" placeholder="Enter username or email" />
            @if (form.get('usernameOrEmail')?.invalid && form.get('usernameOrEmail')?.touched) {
              <span class="form-error">This field is required.</span>
            }
          </div>
          <div class="form-group">
            <label>Password</label>
            <input formControlName="password" type="password" placeholder="Enter password" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="form-error">Password is required.</span>
            }
          </div>
          <button type="submit" class="btn btn--primary btn--full" [disabled]="loading">
            {{ loading ? 'Signing in...' : 'Sign In' }}
          </button>
        </form>
        <p class="auth-card__footer">Don't have an account? <a routerLink="/auth/register">Register</a></p>
      </div>
    </div>
  `,
    styles: [`
    .auth-page { min-height: 100vh; display: flex; align-items: center; justify-content: center; background: linear-gradient(135deg, #2c3e50 0%, var(--primary) 100%); }
    .auth-card { background: #fff; border-radius: 12px; padding: 2.5rem; width: 100%; max-width: 420px; box-shadow: 0 20px 60px rgba(0,0,0,0.2); }
    .auth-card__header { text-align: center; margin-bottom: 2rem; }
    .auth-card__header h1 { font-size: 1.75rem; color: var(--dark); margin-bottom: 0.5rem; }
    .auth-card__header p { color: var(--muted); font-size: 0.95rem; }
    .auth-form { display: flex; flex-direction: column; gap: 1.25rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    .form-group label { font-size: 0.875rem; font-weight: 600; color: var(--dark); }
    .form-group input { padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; transition: border-color 0.2s; outline: none; }
    .form-group input:focus { border-color: var(--primary); }
    .form-error { color: var(--danger); font-size: 0.8rem; }
    .btn--primary { padding: 0.875rem; background: var(--primary); color: #fff; border: none; border-radius: 8px; font-size: 1rem; font-weight: 600; cursor: pointer; transition: background 0.2s; }
    .btn--primary:hover:not(:disabled) { background: var(--primary-dark); }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--full { width: 100%; }
    .auth-card__footer { text-align: center; margin-top: 1.5rem; color: var(--muted); font-size: 0.9rem; }
    .auth-card__footer a { color: var(--primary); font-weight: 600; }
  `]
})
export class LoginComponent {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    private router = inject(Router);
    private toast = inject(ToastService);

    loading = false;

    form = this.fb.group({
        usernameOrEmail: ['', Validators.required],
        password: ['', Validators.required]
    });

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading = true;
        this.auth.login(this.form.value as any).subscribe({
            next: () => { this.toast.success('Welcome back! Login successful.'); this.router.navigate(['/dashboard']); },
            error: (err) => { this.toast.error(err.error?.message || 'Login failed. Please try again.'); this.loading = false; }
        });
    }
}

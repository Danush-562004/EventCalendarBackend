import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-register',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="auth-page">
      <div class="auth-card">
        <div class="auth-card__header">
          <h1>📅 EventCalendar</h1>
          <p>Create your account</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-row">
            <div class="form-group">
              <label>First Name</label>
              <input formControlName="firstName" type="text" placeholder="First name" />
              @if (form.get('firstName')?.invalid && form.get('firstName')?.touched) {
                <span class="form-error">Required.</span>
              }
            </div>
            <div class="form-group">
              <label>Last Name</label>
              <input formControlName="lastName" type="text" placeholder="Last name" />
              @if (form.get('lastName')?.invalid && form.get('lastName')?.touched) {
                <span class="form-error">Required.</span>
              }
            </div>
          </div>
          <div class="form-group">
            <label>Username</label>
            <input formControlName="username" type="text" placeholder="Choose a username" />
            @if (form.get('username')?.invalid && form.get('username')?.touched) {
              <span class="form-error">Username is required.</span>
            }
          </div>
          <div class="form-group">
            <label>Email</label>
            <input formControlName="email" type="email" placeholder="Enter your email" />
            @if (form.get('email')?.invalid && form.get('email')?.touched) {
              <span class="form-error">Valid email is required.</span>
            }
          </div>
          <div class="form-group">
            <label>Password</label>
            <input formControlName="password" type="password" placeholder="Min 6 characters" />
            @if (form.get('password')?.invalid && form.get('password')?.touched) {
              <span class="form-error">Password must be at least 6 characters.</span>
            }
          </div>
          <button type="submit" class="btn btn--primary btn--full" [disabled]="loading">
            {{ loading ? 'Creating account...' : 'Create Account' }}
          </button>
        </form>
        <p class="auth-card__footer">
          Already have an account? <a routerLink="/auth/login">Sign in</a>
        </p>
      </div>
    </div>
  `,
    styles: [`
    .auth-page {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #2c3e50 0%, #3498db 100%);
    }
    .auth-card {
      background: #fff;
      border-radius: 12px;
      padding: 2.5rem;
      width: 100%;
      max-width: 480px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.2);
    }
    .auth-card__header { text-align: center; margin-bottom: 2rem; }
    .auth-card__header h1 { font-size: 1.75rem; color: #2c3e50; margin-bottom: 0.5rem; }
    .auth-card__header p { color: #7f8c8d; font-size: 0.95rem; }
    .auth-form { display: flex; flex-direction: column; gap: 1.1rem; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    .form-group label { font-size: 0.875rem; font-weight: 600; color: #2c3e50; }
    .form-group input {
      padding: 0.75rem 1rem;
      border: 1.5px solid #dde1e7;
      border-radius: 8px;
      font-size: 0.95rem;
      transition: border-color 0.2s;
      outline: none;
    }
    .form-group input:focus { border-color: #3498db; }
    .form-error { color: #e74c3c; font-size: 0.8rem; }
    .btn--primary {
      padding: 0.875rem;
      background: #27ae60;
      color: #fff;
      border: none;
      border-radius: 8px;
      font-size: 1rem;
      font-weight: 600;
      cursor: pointer;
      transition: background 0.2s;
      margin-top: 0.5rem;
    }
    .btn--primary:hover:not(:disabled) { background: #219a52; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--full { width: 100%; }
    .auth-card__footer { text-align: center; margin-top: 1.5rem; color: #7f8c8d; font-size: 0.9rem; }
    .auth-card__footer a { color: #3498db; text-decoration: none; font-weight: 600; }
  `]
})
export class RegisterComponent {
    private fb = inject(FormBuilder);
    private auth = inject(AuthService);
    private router = inject(Router);
    private toast = inject(ToastService);

    loading = false;

    form = this.fb.group({
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        username: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]]
    });

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading = true;

        this.auth.register(this.form.value as any).subscribe({
            next: () => {
                this.toast.success('Account created successfully! Welcome aboard.');
                this.router.navigate(['/dashboard']);
            },
            error: (err) => {
                this.toast.error(err.error?.message || 'Registration failed. Please try again.');
                this.loading = false;
            }
        });
    }
}

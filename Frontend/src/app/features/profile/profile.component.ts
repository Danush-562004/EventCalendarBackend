import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../core/models/api.model';
import { UserResponse } from '../../core/models/auth.model';

@Component({
    selector: 'app-profile',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule],
    template: `
    <div class="page">
        <div class="page__header"><h1>My Profile</h1></div>
        <div class="profile-layout">
            <div class="profile-card">
                <div class="profile-avatar">{{ auth.currentUser()?.firstName?.charAt(0) }}{{ auth.currentUser()?.lastName?.charAt(0) }}</div>
                <h2>{{ auth.currentUser()?.fullName }}</h2>
                <p class="profile-role">{{ auth.currentUser()?.role }}</p>
                <p class="profile-email">{{ auth.currentUser()?.email }}</p>
            </div>
            <div class="forms-col">
                <div class="form-card">
                    <h3>Update Profile</h3>
                    <form [formGroup]="profileForm" (ngSubmit)="updateProfile()">
                        <div class="form-grid">
                            <div class="form-group"><label>First Name</label><input formControlName="firstName" type="text" /></div>
                            <div class="form-group"><label>Last Name</label><input formControlName="lastName" type="text" /></div>
                            <div class="form-group form-group--full"><label>Phone Number</label><input formControlName="phoneNumber" type="tel" /></div>
                            <div class="form-group form-group--full"><label class="checkbox-label"><input formControlName="emailNotifications" type="checkbox" /> Email Notifications</label></div>
                            <div class="form-group form-group--full"><label class="checkbox-label"><input formControlName="pushNotifications" type="checkbox" /> Push Notifications</label></div>
                        </div>
                        <div class="form-actions">
                            <button type="submit" class="btn btn--primary" [disabled]="savingProfile()">{{ savingProfile() ? 'Saving...' : 'Save Changes' }}</button>
                        </div>
                    </form>
                </div>
                <div class="form-card">
                    <h3>Change Password</h3>
                    <form [formGroup]="passwordForm" (ngSubmit)="changePassword()">
                        <div class="form-group">
                            <label>Current Password</label>
                            <input formControlName="currentPassword" type="password" />
                            @if (passwordForm.get('currentPassword')?.invalid && passwordForm.get('currentPassword')?.touched) { <span class="form-error">Required.</span> }
                        </div>
                        <div class="form-group">
                            <label>New Password</label>
                            <input formControlName="newPassword" type="password" />
                            @if (passwordForm.get('newPassword')?.invalid && passwordForm.get('newPassword')?.touched) { <span class="form-error">Min 6 characters.</span> }
                        </div>
                        <div class="form-group">
                            <label>Confirm New Password</label>
                            <input formControlName="confirmPassword" type="password" />
                            @if (passwordForm.hasError('mismatch') && passwordForm.get('confirmPassword')?.touched) { <span class="form-error">Passwords do not match.</span> }
                        </div>
                        <div class="form-actions">
                            <button type="submit" class="btn btn--primary" [disabled]="savingPassword()">{{ savingPassword() ? 'Updating...' : 'Change Password' }}</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
    `,
    styles: [`
        .page { padding: 2rem; max-width: 1000px; margin: 0 auto; }
        .page__header { margin-bottom: 1.5rem; }
        .page__header h1 { font-size: 1.75rem; color: var(--dark); }
        .profile-layout { display: grid; grid-template-columns: 260px 1fr; gap: 1.5rem; }
        @media (max-width: 768px) { .profile-layout { grid-template-columns: 1fr; } }
        .profile-card { background: var(--card); border-radius: 12px; padding: 2rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); text-align: center; height: fit-content; }
        .profile-avatar { width: 80px; height: 80px; border-radius: 50%; background: var(--primary); color: #fff; display: flex; align-items: center; justify-content: center; font-size: 1.75rem; font-weight: 700; margin: 0 auto 1rem; text-transform: uppercase; }
        .profile-card h2 { font-size: 1.1rem; color: var(--dark); margin-bottom: 0.25rem; }
        .profile-role { display: inline-block; background: var(--primary-light); color: var(--primary); padding: 0.2rem 0.75rem; border-radius: 20px; font-size: 0.8rem; font-weight: 600; margin-bottom: 0.5rem; }
        .profile-email { font-size: 0.875rem; color: var(--muted); }
        .forms-col { display: flex; flex-direction: column; gap: 1.5rem; }
        .form-card { background: var(--card); border-radius: 12px; padding: 1.75rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
        .form-card h3 { font-size: 1rem; color: var(--dark); margin-bottom: 1.25rem; padding-bottom: 0.75rem; border-bottom: 1px solid var(--border); }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; }
        .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
        .form-group--full { grid-column: 1 / -1; }
        .form-group label { font-size: 0.875rem; font-weight: 600; color: var(--dark); }
        .form-group input[type="text"], .form-group input[type="password"], .form-group input[type="tel"] { padding: 0.7rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; font-family: inherit; }
        .form-group input:focus { border-color: var(--primary); }
        .checkbox-label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; font-size: 0.9rem; font-weight: 400 !important; }
        .checkbox-label input { width: auto; }
        .form-error { color: var(--danger); font-size: 0.8rem; }
        .form-actions { display: flex; justify-content: flex-end; margin-top: 1.25rem; padding-top: 1rem; border-top: 1px solid var(--border); }
        .btn { padding: 0.6rem 1.25rem; border-radius: 6px; font-size: 0.9rem; font-weight: 500; cursor: pointer; border: none; transition: all 0.2s; }
        .btn--primary { background: var(--primary); color: #fff; }
        .btn--primary:hover:not(:disabled) { background: var(--primary-dark); }
        .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    `]
})
export class ProfileComponent implements OnInit {
    auth = inject(AuthService);
    private fb = inject(FormBuilder);
    private http = inject(HttpClient);
    private toast = inject(ToastService);

    savingProfile = signal(false);
    savingPassword = signal(false);

    profileForm = this.fb.group({
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        phoneNumber: [''],
        emailNotifications: [true],
        pushNotifications: [true]
    });

    passwordForm = this.fb.group({
        currentPassword: ['', Validators.required],
        newPassword: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required]
    }, { validators: this.passwordMatchValidator });

    ngOnInit() {
        const user = this.auth.currentUser();
        if (user) {
            this.profileForm.patchValue({ firstName: user.firstName, lastName: user.lastName, phoneNumber: user.phoneNumber ?? '', emailNotifications: user.emailNotifications, pushNotifications: user.pushNotifications });
        }
    }

    updateProfile() {
        if (this.profileForm.invalid) { this.profileForm.markAllAsTouched(); return; }
        this.savingProfile.set(true);
        const userId = this.auth.currentUser()!.id;
        this.http.put<ApiResponse<UserResponse>>(`${environment.apiUrl}/users/${userId}`, this.profileForm.value).subscribe({
            next: () => { this.toast.success('Profile updated successfully.'); this.savingProfile.set(false); },
            error: err => { this.toast.error(err.error?.message || 'Failed to update profile.'); this.savingProfile.set(false); }
        });
    }

    changePassword() {
        if (this.passwordForm.invalid) { this.passwordForm.markAllAsTouched(); return; }
        this.savingPassword.set(true);
        const userId = this.auth.currentUser()!.id;
        const { currentPassword, newPassword } = this.passwordForm.value;
        this.http.post<ApiResponse<null>>(`${environment.apiUrl}/users/${userId}/change-password`, { currentPassword, newPassword }).subscribe({
            next: () => { this.toast.success('Password changed successfully.'); this.passwordForm.reset(); this.savingPassword.set(false); },
            error: err => { this.toast.error(err.error?.message || 'Failed to change password.'); this.savingPassword.set(false); }
        });
    }

    private passwordMatchValidator(group: any) {
        const pw = group.get('newPassword')?.value;
        const confirm = group.get('confirmPassword')?.value;
        return pw === confirm ? null : { mismatch: true };
    }
}

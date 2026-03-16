import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { VenueService } from '../../../core/services/venue.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-venue-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="page">
        <div class="page__header">
            <h1>{{ isEdit ? 'Edit Venue' : 'Add Venue' }}</h1>
            <a routerLink="/venues" class="btn btn--outline">← Back</a>
        </div>
        <div class="form-card">
            <form [formGroup]="form" (ngSubmit)="onSubmit()">
                <div class="form-grid">
                    <div class="form-group form-group--full">
                        <label>Venue Name *</label>
                        <input formControlName="name" type="text" placeholder="Venue name" />
                        @if (form.get('name')?.invalid && form.get('name')?.touched) { <span class="form-error">Name is required.</span> }
                    </div>
                    <div class="form-group form-group--full"><label>Address *</label><input formControlName="address" type="text" /></div>
                    <div class="form-group"><label>City *</label><input formControlName="city" type="text" /></div>
                    <div class="form-group"><label>State *</label><input formControlName="state" type="text" /></div>
                    <div class="form-group"><label>Country *</label><input formControlName="country" type="text" /></div>
                    <div class="form-group"><label>Zip Code</label><input formControlName="zipCode" type="text" /></div>
                    <div class="form-group"><label>Capacity *</label><input formControlName="capacity" type="number" min="1" /></div>
                    <div class="form-group"><label>Contact Email</label><input formControlName="contactEmail" type="email" /></div>
                    <div class="form-group"><label>Contact Phone</label><input formControlName="contactPhone" type="tel" /></div>
                    <div class="form-group form-group--full"><label>Description</label><textarea formControlName="description" rows="3"></textarea></div>
                </div>
                <div class="form-actions">
                    <a routerLink="/venues" class="btn btn--outline">Cancel</a>
                    <button type="submit" class="btn btn--primary" [disabled]="loading()">{{ loading() ? 'Saving...' : (isEdit ? 'Update Venue' : 'Add Venue') }}</button>
                </div>
            </form>
        </div>
    </div>
    `,
    styles: [`
        .page { padding: 2rem; max-width: 800px; margin: 0 auto; }
        .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page__header h1 { font-size: 1.75rem; color: var(--dark); }
        .form-card { background: var(--card); border-radius: 12px; padding: 2rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
        .form-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 1.25rem; }
        .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
        .form-group--full { grid-column: 1 / -1; }
        .form-group label { font-size: 0.875rem; font-weight: 600; color: var(--dark); }
        .form-group input, .form-group textarea { padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; font-family: inherit; }
        .form-group input:focus, .form-group textarea:focus { border-color: var(--primary); }
        .form-error { color: var(--danger); font-size: 0.8rem; }
        .form-actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid var(--border); }
        .btn { padding: 0.6rem 1.25rem; border-radius: 6px; text-decoration: none; font-size: 0.9rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
        .btn--primary { background: var(--primary); color: #fff; }
        .btn--primary:hover:not(:disabled) { background: var(--primary-dark); text-decoration: none; }
        .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
        .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
    `]
})
export class VenueFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private venueService = inject(VenueService);
    private toast = inject(ToastService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    loading = signal(false);
    isEdit = false;
    venueId?: number;

    form = this.fb.group({
        name: ['', Validators.required],
        address: ['', Validators.required],
        city: ['', Validators.required],
        state: ['', Validators.required],
        country: ['', Validators.required],
        zipCode: [''],
        capacity: [1, [Validators.required, Validators.min(1)]],
        description: [''],
        contactEmail: [''],
        contactPhone: ['']
    });

    ngOnInit() {
        this.venueId = this.route.snapshot.params['id'];
        if (this.venueId) {
            this.isEdit = true;
            this.venueService.getById(this.venueId).subscribe(res => this.form.patchValue(res.data as any));
        }
    }

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading.set(true);
        const obs = this.isEdit ? this.venueService.update(this.venueId!, this.form.value) : this.venueService.create(this.form.value);
        obs.subscribe({
            next: () => { this.toast.success(this.isEdit ? 'Venue updated.' : 'Venue created.'); this.router.navigate(['/venues']); },
            error: err => { this.toast.error(err.error?.message || 'Failed to save venue.'); this.loading.set(false); }
        });
    }
}

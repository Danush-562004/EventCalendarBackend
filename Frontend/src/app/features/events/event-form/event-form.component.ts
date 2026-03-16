import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { EventService } from '../../../core/services/event.service';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { CategoryResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-event-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="page">
      <div class="page__header">
        <h1>{{ isEdit ? 'Edit Event' : 'Create Event' }}</h1>
        <a routerLink="/events" class="btn btn--outline">← Back</a>
      </div>
      <div class="form-card">
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-grid">
            <div class="form-group form-group--full">
              <label>Title *</label>
              <input formControlName="title" type="text" placeholder="Event title" />
              @if (form.get('title')?.invalid && form.get('title')?.touched) { <span class="form-error">Title is required.</span> }
            </div>
            <div class="form-group form-group--full">
              <label>Description</label>
              <textarea formControlName="description" rows="3" placeholder="Event description"></textarea>
            </div>
            <div class="form-group">
              <label>Start Date & Time *</label>
              <input formControlName="startDateTime" type="datetime-local" />
            </div>
            <div class="form-group">
              <label>End Date & Time *</label>
              <input formControlName="endDateTime" type="datetime-local" />
            </div>
            <div class="form-group">
              <label>Location</label>
              <input formControlName="location" type="text" placeholder="Event location" />
            </div>
            <div class="form-group">
              <label>Category *</label>
              <select formControlName="categoryId">
                <option value="">Select category</option>
                @for (cat of categories(); track cat.id) { <option [value]="cat.id">{{ cat.name }}</option> }
              </select>
            </div>
            <div class="form-group">
              <label>Privacy</label>
              <select formControlName="privacy">
                <option value="0">Public</option>
                <option value="1">Private</option>
                <option value="2">Invite Only</option>
              </select>
            </div>
            <div class="form-group">
              <label>Max Attendees (0 = unlimited)</label>
              <input formControlName="maxAttendees" type="number" min="0" />
            </div>
            <div class="form-group form-group--full">
              <label class="checkbox-label">
                <input formControlName="reminderEnabled" type="checkbox" /> Enable Reminder
              </label>
            </div>
            @if (form.get('reminderEnabled')?.value) {
              <div class="form-group">
                <label>Reminder (minutes before)</label>
                <input formControlName="reminderMinutesBefore" type="number" min="1" placeholder="e.g. 30" />
              </div>
            }
          </div>
          <div class="form-actions">
            <a routerLink="/events" class="btn btn--outline">Cancel</a>
            <button type="submit" class="btn btn--primary" [disabled]="loading()">
              {{ loading() ? 'Saving...' : (isEdit ? 'Update Event' : 'Create Event') }}
            </button>
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
    .form-group input, .form-group select, .form-group textarea { padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; font-family: inherit; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { border-color: var(--primary); }
    .checkbox-label { display: flex; align-items: center; gap: 0.5rem; cursor: pointer; font-size: 0.9rem; }
    .checkbox-label input { width: auto; }
    .form-error { color: var(--danger); font-size: 0.8rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 1rem; margin-top: 1.5rem; padding-top: 1.5rem; border-top: 1px solid var(--border); }
    .btn { padding: 0.6rem 1.25rem; border-radius: 6px; text-decoration: none; font-size: 0.9rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
    .btn--primary { background: var(--primary); color: #fff; }
    .btn--primary:hover:not(:disabled) { background: var(--primary-dark); text-decoration: none; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
  `]
})
export class EventFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private eventService = inject(EventService);
    private categoryService = inject(CategoryService);
    private toast = inject(ToastService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    categories = signal<CategoryResponse[]>([]);
    loading = signal(false);
    isEdit = false;
    eventId?: number;

    form = this.fb.group({
        title: ['', Validators.required],
        description: [''],
        startDateTime: ['', Validators.required],
        endDateTime: ['', Validators.required],
        location: [''],
        categoryId: ['', Validators.required],
        privacy: [0],
        maxAttendees: [0],
        reminderEnabled: [false],
        reminderMinutesBefore: [null as number | null]
    });

    ngOnInit() {
        this.categoryService.getAll().subscribe(res => this.categories.set(res.data.items));
        this.eventId = this.route.snapshot.params['id'];
        if (this.eventId) {
            this.isEdit = true;
            this.eventService.getById(this.eventId).subscribe(res => {
                const e = res.data;
                this.form.patchValue({
                    title: e.title, description: e.description,
                    startDateTime: e.startDateTime.slice(0, 16), endDateTime: e.endDateTime.slice(0, 16),
                    location: e.location, categoryId: String(e.category.id),
                    maxAttendees: e.maxAttendees, reminderEnabled: e.reminderEnabled,
                    reminderMinutesBefore: e.reminderMinutesBefore ?? null
                });
            });
        }
    }

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading.set(true);
        const payload = { ...this.form.value, categoryId: Number(this.form.value.categoryId), privacy: Number(this.form.value.privacy) };
        const obs = this.isEdit ? this.eventService.update(this.eventId!, payload) : this.eventService.create(payload);
        obs.subscribe({
            next: () => { this.toast.success(this.isEdit ? 'Event updated.' : 'Event created.'); this.router.navigate(['/events']); },
            error: err => { this.toast.error(err.error?.message || 'Failed to save event.'); this.loading.set(false); }
        });
    }
}

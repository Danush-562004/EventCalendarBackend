import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReminderService } from '../../../core/services/reminder.service';
import { EventService } from '../../../core/services/event.service';
import { ToastService } from '../../../core/services/toast.service';
import { EventResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-reminder-form',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, RouterLink],
    template: `
    <div class="page">
      <div class="page__header">
        <h1>{{ isEdit ? 'Edit Reminder' : 'Create Reminder' }}</h1>
        <a routerLink="/reminders" class="btn btn--outline">← Back</a>
      </div>
      <div class="form-card">
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Title *</label>
            <input formControlName="title" type="text" placeholder="Reminder title" />
            @if (form.get('title')?.invalid && form.get('title')?.touched) { <span class="form-error">Title is required.</span> }
          </div>
          <div class="form-group">
            <label>Message</label>
            <textarea formControlName="message" rows="3" placeholder="Optional message"></textarea>
          </div>
          <div class="form-group">
            <label>Reminder Date & Time *</label>
            <input formControlName="reminderDateTime" type="datetime-local" />
          </div>
          <div class="form-group">
            <label>Notification Type</label>
            <select formControlName="type">
              <option value="0">Email</option>
              <option value="1">Push</option>
              <option value="2">Both</option>
            </select>
          </div>
          @if (!isEdit) {
            <div class="form-group">
              <label>Event *</label>
              <select formControlName="eventId">
                <option value="">Select event</option>
                @for (event of myEvents(); track event.id) { <option [value]="event.id">{{ event.title }}</option> }
              </select>
              @if (form.get('eventId')?.invalid && form.get('eventId')?.touched) { <span class="form-error">Please select an event.</span> }
            </div>
          }
          <div class="form-actions">
            <a routerLink="/reminders" class="btn btn--outline">Cancel</a>
            <button type="submit" class="btn btn--primary" [disabled]="loading()">
              {{ loading() ? 'Saving...' : (isEdit ? 'Update' : 'Create Reminder') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
    styles: [`
    .page { padding: 2rem; max-width: 600px; margin: 0 auto; }
    .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page__header h1 { font-size: 1.75rem; color: var(--dark); }
    .form-card { background: var(--card); border-radius: 12px; padding: 2rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); display: flex; flex-direction: column; gap: 1.25rem; }
    .form-group { display: flex; flex-direction: column; gap: 0.4rem; }
    .form-group label { font-size: 0.875rem; font-weight: 600; color: var(--dark); }
    .form-group input, .form-group select, .form-group textarea { padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; font-size: 0.95rem; outline: none; font-family: inherit; }
    .form-group input:focus, .form-group select:focus, .form-group textarea:focus { border-color: var(--primary); }
    .form-error { color: var(--danger); font-size: 0.8rem; }
    .form-actions { display: flex; justify-content: flex-end; gap: 1rem; padding-top: 1rem; border-top: 1px solid var(--border); }
    .btn { padding: 0.6rem 1.25rem; border-radius: 6px; text-decoration: none; font-size: 0.9rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
    .btn--primary { background: var(--primary); color: #fff; }
    .btn--primary:hover:not(:disabled) { background: var(--primary-dark); text-decoration: none; }
    .btn--primary:disabled { opacity: 0.6; cursor: not-allowed; }
    .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
  `]
})
export class ReminderFormComponent implements OnInit {
    private fb = inject(FormBuilder);
    private reminderService = inject(ReminderService);
    private eventService = inject(EventService);
    private toast = inject(ToastService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    myEvents = signal<EventResponse[]>([]);
    loading = signal(false);
    isEdit = false;
    reminderId?: number;

    form = this.fb.group({
        title: ['', Validators.required],
        message: [''],
        reminderDateTime: ['', Validators.required],
        type: [0],
        eventId: ['']
    });

    ngOnInit() {
        this.eventService.getMyEvents().subscribe(res => this.myEvents.set(res.data));
        this.reminderId = this.route.snapshot.params['id'];
        if (this.reminderId) {
            this.isEdit = true;
            this.reminderService.getById(this.reminderId).subscribe(res => {
                const r = res.data;
                this.form.patchValue({ title: r.title, message: r.message, reminderDateTime: r.reminderDateTime.slice(0, 16) });
            });
        }
    }

    onSubmit() {
        if (this.form.invalid) { this.form.markAllAsTouched(); return; }
        this.loading.set(true);
        const payload = { ...this.form.value, type: Number(this.form.value.type), eventId: Number(this.form.value.eventId) };
        const obs = this.isEdit ? this.reminderService.update(this.reminderId!, payload) : this.reminderService.create(payload);
        obs.subscribe({
            next: () => { this.toast.success(this.isEdit ? 'Reminder updated.' : 'Reminder created.'); this.router.navigate(['/reminders']); },
            error: err => { this.toast.error(err.error?.message || 'Failed to save reminder.'); this.loading.set(false); }
        });
    }
}

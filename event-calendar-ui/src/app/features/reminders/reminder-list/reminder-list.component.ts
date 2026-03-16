import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ReminderService } from '../../../core/services/reminder.service';
import { ToastService } from '../../../core/services/toast.service';
import { ReminderResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-reminder-list',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page">
      <div class="page__header">
        <h1>My Reminders</h1>
        <a routerLink="/reminders/create" class="btn btn--primary">+ Add Reminder</a>
      </div>
      @if (loading()) {
        <div class="loading">Loading reminders...</div>
      } @else if (reminders().length === 0) {
        <div class="empty-state">
          <span>🔔</span>
          <p>No reminders yet. Create one to stay on top of your events!</p>
          <a routerLink="/reminders/create" class="btn btn--primary">Create Reminder</a>
        </div>
      } @else {
        <div class="reminder-list">
          @for (r of reminders(); track r.id) {
            <div class="reminder-card" [class.reminder-card--sent]="r.isSent">
              <div class="reminder-card__icon">{{ typeIcons[r.type] || '🔔' }}</div>
              <div class="reminder-card__body">
                <h3>{{ r.title }}</h3>
                <p class="reminder-card__event">📅 {{ r.eventTitle }}</p>
                @if (r.message) { <p class="reminder-card__msg">{{ r.message }}</p> }
                <div class="reminder-card__meta">
                  <span>⏰ {{ r.reminderDateTime | date:'medium' }}</span>
                  <span class="type-badge type-badge--{{ r.type.toLowerCase() }}">{{ r.type }}</span>
                  @if (r.isSent) { <span class="sent-badge">✓ Sent</span> }
                </div>
              </div>
              <div class="reminder-card__actions">
                <a [routerLink]="['/reminders', r.id, 'edit']" class="btn btn--sm btn--warning">Edit</a>
                <button class="btn btn--sm btn--danger" (click)="deleteReminder(r.id)">Delete</button>
              </div>
            </div>
          }
        </div>
        <div class="pagination">
          <button class="btn btn--outline" [disabled]="page() === 1" (click)="changePage(page() - 1)">← Prev</button>
          <span>Page {{ page() }} of {{ totalPages() }}</span>
          <button class="btn btn--outline" [disabled]="page() >= totalPages()" (click)="changePage(page() + 1)">Next →</button>
        </div>
      }
    </div>
  `,
    styles: [`
    .page { padding: 2rem; max-width: 900px; margin: 0 auto; }
    .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page__header h1 { font-size: 1.75rem; color: #2c3e50; }
    .loading { text-align: center; padding: 3rem; color: #7f8c8d; }
    .empty-state { text-align: center; padding: 4rem 2rem; color: #7f8c8d; display: flex; flex-direction: column; align-items: center; gap: 1rem; }
    .empty-state span { font-size: 3rem; }
    .reminder-list { display: flex; flex-direction: column; gap: 1rem; }
    .reminder-card { display: flex; align-items: flex-start; gap: 1rem; background: #fff; border-radius: 12px; padding: 1.25rem; box-shadow: 0 2px 8px rgba(0,0,0,0.06); border-left: 4px solid #3498db; }
    .reminder-card--sent { opacity: 0.65; border-left-color: #27ae60; }
    .reminder-card__icon { font-size: 1.75rem; flex-shrink: 0; }
    .reminder-card__body { flex: 1; }
    .reminder-card__body h3 { font-size: 1rem; color: #2c3e50; margin-bottom: 0.25rem; }
    .reminder-card__event { font-size: 0.85rem; color: #7f8c8d; margin-bottom: 0.25rem; }
    .reminder-card__msg { font-size: 0.875rem; color: #555; margin-bottom: 0.5rem; }
    .reminder-card__meta { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; font-size: 0.8rem; color: #7f8c8d; }
    .type-badge { padding: 0.15rem 0.5rem; border-radius: 20px; font-size: 0.75rem; font-weight: 500; }
    .type-badge--email { background: #e3f2fd; color: #1565c0; }
    .type-badge--push { background: #f3e5f5; color: #6a1b9a; }
    .type-badge--both { background: #e8f5e9; color: #2e7d32; }
    .sent-badge { background: #e8f5e9; color: #27ae60; padding: 0.15rem 0.5rem; border-radius: 20px; font-size: 0.75rem; }
    .reminder-card__actions { display: flex; gap: 0.5rem; flex-shrink: 0; }
    .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
    .btn--primary { background: #3498db; color: #fff; }
    .btn--outline { background: transparent; border: 1.5px solid #bdc3c7; color: #7f8c8d; }
    .btn--sm { padding: 0.3rem 0.65rem; font-size: 0.8rem; }
    .btn--warning { background: #f39c12; color: #fff; }
    .btn--danger { background: #e74c3c; color: #fff; }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 2rem; }
  `]
})
export class ReminderListComponent implements OnInit {
    private reminderService = inject(ReminderService);
    private toast = inject(ToastService);

    reminders = signal<ReminderResponse[]>([]);
    loading = signal(true);
    page = signal(1);
    totalPages = signal(1);

    typeIcons: Record<string, string> = { Email: '📧', Push: '📱', Both: '🔔' };

    ngOnInit() { this.loadReminders(); }

    loadReminders() {
        this.loading.set(true);
        this.reminderService.getMyReminders(this.page(), 20).subscribe({
            next: res => {
                this.reminders.set(res.data.items);
                this.totalPages.set(res.data.totalPages);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    changePage(p: number) { this.page.set(p); this.loadReminders(); }

    deleteReminder(id: number) {
        if (!confirm('Delete this reminder?')) return;
        this.reminderService.delete(id).subscribe({
            next: () => { this.toast.success('Reminder deleted.'); this.loadReminders(); },
            error: err => this.toast.error(err.error?.message || 'Failed to delete.')
        });
    }
}

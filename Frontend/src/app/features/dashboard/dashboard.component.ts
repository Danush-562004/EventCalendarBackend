import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { EventService } from '../../core/services/event.service';
import { ReminderService } from '../../core/services/reminder.service';
import { EventResponse, ReminderResponse } from '../../core/models/api.model';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="dashboard">
      <div class="dashboard__hero">
        <h1>Welcome back, {{ auth.currentUser()?.firstName }}! 👋</h1>
        <p>Here's what's happening with your events.</p>
      </div>
      <div class="dashboard__stats">
        <div class="stat-card stat-card--blue">
          <span class="stat-card__icon">📅</span>
          <div><div class="stat-card__value">{{ upcomingEvents().length }}</div><div class="stat-card__label">Upcoming Events</div></div>
        </div>
        <div class="stat-card stat-card--green">
          <span class="stat-card__icon">🔔</span>
          <div><div class="stat-card__value">{{ reminders().length }}</div><div class="stat-card__label">Active Reminders</div></div>
        </div>
        <div class="stat-card stat-card--orange">
          <span class="stat-card__icon">👤</span>
          <div><div class="stat-card__value">{{ auth.currentUser()?.role }}</div><div class="stat-card__label">Your Role</div></div>
        </div>
      </div>
      <div class="dashboard__sections">
        <div class="section">
          <div class="section__header">
            <h2>My Upcoming Events</h2>
            <a routerLink="/events/create" class="btn btn--primary">+ New Event</a>
          </div>
          @if (loadingEvents()) {
            <div class="loading">Loading events...</div>
          } @else if (upcomingEvents().length === 0) {
            <div class="empty-state">No upcoming events. <a routerLink="/events/create">Create one!</a></div>
          } @else {
            <div class="event-list">
              @for (event of upcomingEvents(); track event.id) {
                <div class="event-card" [style.border-left-color]="event.category.colorCode">
                  <div class="event-card__date">
                    <span class="event-card__day">{{ event.startDateTime | date:'d' }}</span>
                    <span class="event-card__month">{{ event.startDateTime | date:'MMM' }}</span>
                  </div>
                  <div class="event-card__info">
                    <h3>{{ event.title }}</h3>
                    <p>{{ event.startDateTime | date:'shortTime' }} · {{ event.location || 'No location' }}</p>
                    <span class="badge" [style.background]="event.category.colorCode">{{ event.category.name }}</span>
                  </div>
                  <a [routerLink]="['/events', event.id]" class="btn btn--sm">View</a>
                </div>
              }
            </div>
          }
        </div>
        <div class="section">
          <div class="section__header">
            <h2>Upcoming Reminders</h2>
            <a routerLink="/reminders" class="btn btn--outline">View All</a>
          </div>
          @if (reminders().length === 0) {
            <div class="empty-state">No reminders set.</div>
          } @else {
            @for (r of reminders().slice(0, 5); track r.id) {
              <div class="reminder-item">
                <span class="reminder-item__icon">🔔</span>
                <div>
                  <div class="reminder-item__title">{{ r.title }}</div>
                  <div class="reminder-item__meta">{{ r.eventTitle }} · {{ r.reminderDateTime | date:'medium' }}</div>
                </div>
              </div>
            }
          }
        </div>
      </div>
    </div>
  `,
    styles: [`
    .dashboard { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .dashboard__hero { margin-bottom: 2rem; }
    .dashboard__hero h1 { font-size: 1.75rem; color: var(--dark); margin-bottom: 0.25rem; }
    .dashboard__hero p { color: var(--muted); }
    .dashboard__stats { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 1.25rem; margin-bottom: 2.5rem; }
    .stat-card { display: flex; align-items: center; gap: 1rem; padding: 1.5rem; border-radius: 12px; color: #fff; }
    .stat-card--blue { background: linear-gradient(135deg, var(--info), #1d4ed8); }
    .stat-card--green { background: linear-gradient(135deg, var(--success), #059669); }
    .stat-card--orange { background: linear-gradient(135deg, var(--warning), #d97706); }
    .stat-card__icon { font-size: 2rem; }
    .stat-card__value { font-size: 1.75rem; font-weight: 700; }
    .stat-card__label { font-size: 0.85rem; opacity: 0.9; }
    .dashboard__sections { display: grid; grid-template-columns: 2fr 1fr; gap: 2rem; }
    @media (max-width: 768px) { .dashboard__sections { grid-template-columns: 1fr; } }
    .section { background: var(--card); border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
    .section__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.25rem; }
    .section__header h2 { font-size: 1.1rem; color: var(--dark); }
    .event-list { display: flex; flex-direction: column; gap: 0.75rem; }
    .event-card { display: flex; align-items: center; gap: 1rem; padding: 1rem; border-radius: 8px; border-left: 4px solid var(--primary); background: var(--bg); }
    .event-card__date { text-align: center; min-width: 40px; }
    .event-card__day { display: block; font-size: 1.4rem; font-weight: 700; color: var(--dark); line-height: 1; }
    .event-card__month { font-size: 0.75rem; color: var(--muted); text-transform: uppercase; }
    .event-card__info { flex: 1; }
    .event-card__info h3 { font-size: 0.95rem; color: var(--dark); margin-bottom: 0.2rem; }
    .event-card__info p { font-size: 0.8rem; color: var(--muted); margin-bottom: 0.4rem; }
    .badge { display: inline-block; padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; color: #fff; }
    .reminder-item { display: flex; align-items: flex-start; gap: 0.75rem; padding: 0.75rem 0; border-bottom: 1px solid var(--border); }
    .reminder-item:last-child { border-bottom: none; }
    .reminder-item__icon { font-size: 1.25rem; }
    .reminder-item__title { font-size: 0.9rem; font-weight: 600; color: var(--dark); }
    .reminder-item__meta { font-size: 0.8rem; color: var(--muted); }
    .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; }
    .btn--primary { background: var(--primary); color: #fff; }
    .btn--primary:hover { background: var(--primary-dark); text-decoration: none; }
    .btn--outline { background: transparent; border: 1.5px solid var(--primary); color: var(--primary); }
    .btn--sm { padding: 0.35rem 0.75rem; font-size: 0.8rem; background: var(--primary); color: #fff; border-radius: 4px; }
    .loading, .empty-state { color: var(--muted); font-size: 0.9rem; padding: 1rem 0; }
    .empty-state a { color: var(--primary); }
  `]
})
export class DashboardComponent implements OnInit {
    auth = inject(AuthService);
    private eventService = inject(EventService);
    private reminderService = inject(ReminderService);

    upcomingEvents = signal<EventResponse[]>([]);
    reminders = signal<ReminderResponse[]>([]);
    loadingEvents = signal(true);

    ngOnInit() {
        this.eventService.getMyEvents().subscribe({
            next: res => {
                const now = new Date();
                this.upcomingEvents.set(res.data.filter(e => new Date(e.startDateTime) >= now).slice(0, 5));
                this.loadingEvents.set(false);
            },
            error: () => this.loadingEvents.set(false)
        });
        this.reminderService.getMyReminders(1, 10).subscribe({
            next: res => this.reminders.set(res.data.items.filter(r => !r.isSent))
        });
    }
}

import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { EventService } from '../../../core/services/event.service';
import { EventResponse } from '../../../core/models/api.model';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-event-list',
    standalone: true,
    imports: [CommonModule, RouterLink, FormsModule],
    template: `
    <div class="page">
      <div class="page__header">
        <h1>Events</h1>
        <a routerLink="/events/create" class="btn btn--primary">+ Create Event</a>
      </div>
      <div class="filters">
        <input [(ngModel)]="keyword" (ngModelChange)="onSearch()" placeholder="Search events..." class="search-input" />
      </div>
      @if (loading()) {
        <div class="loading-spinner">Loading events...</div>
      } @else {
        <div class="event-grid">
          @for (event of events(); track event.id) {
            <div class="event-card" [style.border-top-color]="event.category.colorCode">
              <div class="event-card__header">
                <span class="badge" [style.background]="event.category.colorCode">{{ event.category.name }}</span>
                <span class="privacy-badge privacy-badge--{{ event.privacy.toLowerCase() }}">{{ event.privacy }}</span>
              </div>
              <h3 class="event-card__title">{{ event.title }}</h3>
              <p class="event-card__desc">{{ event.description || 'No description provided.' }}</p>
              <div class="event-card__meta">
                <span>📅 {{ event.startDateTime | date:'mediumDate' }}</span>
                <span>🕐 {{ event.startDateTime | date:'shortTime' }}</span>
                @if (event.location) { <span>📍 {{ event.location }}</span> }
              </div>
              <div class="event-card__footer">
                <span class="organizer">by {{ event.organizerName }}</span>
                <div class="event-card__actions">
                  <a [routerLink]="['/events', event.id]" class="btn btn--sm btn--outline">View</a>
                  @if (auth.currentUser()?.id === event.userId) {
                    <a [routerLink]="['/events', event.id, 'edit']" class="btn btn--sm btn--warning">Edit</a>
                    <button class="btn btn--sm btn--danger" (click)="deleteEvent(event.id)">Delete</button>
                  }
                </div>
              </div>
            </div>
          }
        </div>
        @if (events().length === 0) { <div class="empty-state">No events found.</div> }
        <div class="pagination">
          <button class="btn btn--outline" [disabled]="page() === 1" (click)="changePage(page() - 1)">← Prev</button>
          <span>Page {{ page() }} of {{ totalPages() }}</span>
          <button class="btn btn--outline" [disabled]="page() >= totalPages()" (click)="changePage(page() + 1)">Next →</button>
        </div>
      }
    </div>
  `,
    styles: [`
    .page { padding: 2rem; max-width: 1200px; margin: 0 auto; }
    .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
    .page__header h1 { font-size: 1.75rem; color: var(--dark); }
    .filters { margin-bottom: 1.5rem; }
    .search-input { padding: 0.75rem 1rem; border: 1.5px solid var(--border); border-radius: 8px; width: 100%; max-width: 400px; font-size: 0.95rem; outline: none; }
    .search-input:focus { border-color: var(--primary); }
    .event-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1.5rem; }
    .event-card { background: var(--card); border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); border-top: 4px solid var(--primary); display: flex; flex-direction: column; gap: 0.75rem; }
    .event-card__header { display: flex; justify-content: space-between; align-items: center; }
    .badge { padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; color: #fff; }
    .privacy-badge { padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 500; }
    .privacy-badge--public { background: #dcfce7; color: #166534; }
    .privacy-badge--private { background: #fee2e2; color: #991b1b; }
    .privacy-badge--inviteonly { background: #fef3c7; color: #92400e; }
    .event-card__title { font-size: 1.05rem; font-weight: 700; color: var(--dark); }
    .event-card__desc { font-size: 0.875rem; color: var(--muted); line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
    .event-card__meta { display: flex; flex-direction: column; gap: 0.25rem; font-size: 0.8rem; color: var(--muted); }
    .event-card__footer { display: flex; justify-content: space-between; align-items: center; margin-top: auto; padding-top: 0.75rem; border-top: 1px solid var(--border); }
    .organizer { font-size: 0.8rem; color: var(--muted); }
    .event-card__actions { display: flex; gap: 0.5rem; }
    .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
    .btn--primary { background: var(--primary); color: #fff; }
    .btn--primary:hover { background: var(--primary-dark); text-decoration: none; }
    .btn--outline { background: transparent; border: 1.5px solid var(--primary); color: var(--primary); }
    .btn--sm { padding: 0.3rem 0.65rem; font-size: 0.8rem; }
    .btn--warning { background: var(--warning); color: #fff; }
    .btn--danger { background: var(--danger); color: #fff; }
    .btn:disabled { opacity: 0.5; cursor: not-allowed; }
    .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 2rem; }
    .loading-spinner, .empty-state { text-align: center; padding: 3rem; color: var(--muted); }
  `]
})
export class EventListComponent implements OnInit {
    private eventService = inject(EventService);
    private toast = inject(ToastService);
    auth = inject(AuthService);

    events = signal<EventResponse[]>([]);
    loading = signal(true);
    page = signal(1);
    totalPages = signal(1);
    keyword = '';
    private searchTimeout: any;

    ngOnInit() { this.loadEvents(); }

    loadEvents() {
        this.loading.set(true);
        const obs = this.keyword
            ? this.eventService.search({ keyword: this.keyword, page: this.page(), pageSize: 12 })
            : this.eventService.getAll(this.page(), 12);
        obs.subscribe({
            next: res => { this.events.set(res.data.items); this.totalPages.set(res.data.totalPages); this.loading.set(false); },
            error: () => this.loading.set(false)
        });
    }

    onSearch() {
        clearTimeout(this.searchTimeout);
        this.searchTimeout = setTimeout(() => { this.page.set(1); this.loadEvents(); }, 400);
    }

    changePage(p: number) { this.page.set(p); this.loadEvents(); }

    deleteEvent(id: number) {
        if (!confirm('Delete this event?')) return;
        this.eventService.delete(id).subscribe({
            next: () => { this.toast.success('Event deleted.'); this.loadEvents(); },
            error: err => this.toast.error(err.error?.message || 'Failed to delete event.')
        });
    }
}

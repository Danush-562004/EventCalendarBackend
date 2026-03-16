import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EventService } from '../../../core/services/event.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-event-detail',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page">
      @if (loading()) {
        <div class="loading">Loading event...</div>
      } @else if (event()) {
        <div class="event-detail">
          <div class="event-detail__header" [style.border-left-color]="event()!.category.colorCode">
            <div>
              <span class="badge" [style.background]="event()!.category.colorCode">{{ event()!.category.name }}</span>
              <h1>{{ event()!.title }}</h1>
              <p class="organizer">Organized by {{ event()!.organizerName }}</p>
            </div>
            <div class="event-detail__actions">
              <a routerLink="/events" class="btn btn--outline">← Back</a>
              @if (auth.currentUser()?.id === event()!.userId) {
                <a [routerLink]="['/events', event()!.id, 'edit']" class="btn btn--warning">Edit</a>
                <button class="btn btn--danger" (click)="deleteEvent()">Delete</button>
              }
            </div>
          </div>
          <div class="event-detail__body">
            <div class="info-grid">
              <div class="info-item"><span class="info-item__label">📅 Start</span><span>{{ event()!.startDateTime | date:'full' }}</span></div>
              <div class="info-item"><span class="info-item__label">🏁 End</span><span>{{ event()!.endDateTime | date:'full' }}</span></div>
              @if (event()!.location) { <div class="info-item"><span class="info-item__label">📍 Location</span><span>{{ event()!.location }}</span></div> }
              <div class="info-item"><span class="info-item__label">🔒 Privacy</span><span>{{ event()!.privacy }}</span></div>
              @if (event()!.maxAttendees > 0) { <div class="info-item"><span class="info-item__label">👥 Max Attendees</span><span>{{ event()!.maxAttendees }}</span></div> }
              <div class="info-item"><span class="info-item__label">🎫 Tickets Sold</span><span>{{ event()!.ticketCount }}</span></div>
            </div>
            @if (event()!.description) {
              <div class="description"><h3>About this event</h3><p>{{ event()!.description }}</p></div>
            }
          </div>
        </div>
      }
    </div>
  `,
    styles: [`
    .page { padding: 2rem; max-width: 900px; margin: 0 auto; }
    .loading { text-align: center; padding: 3rem; color: var(--muted); }
    .event-detail { background: var(--card); border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.06); }
    .event-detail__header { padding: 2rem; border-left: 6px solid var(--primary); display: flex; justify-content: space-between; align-items: flex-start; background: var(--bg); }
    .event-detail__header h1 { font-size: 1.75rem; color: var(--dark); margin: 0.5rem 0; }
    .organizer { color: var(--muted); font-size: 0.9rem; }
    .event-detail__actions { display: flex; gap: 0.75rem; flex-shrink: 0; }
    .event-detail__body { padding: 2rem; }
    .info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 1.25rem; margin-bottom: 2rem; }
    .info-item { display: flex; flex-direction: column; gap: 0.25rem; }
    .info-item__label { font-size: 0.8rem; font-weight: 600; color: var(--muted); text-transform: uppercase; letter-spacing: 0.5px; }
    .info-item span:last-child { font-size: 0.95rem; color: var(--dark); }
    .description h3 { font-size: 1rem; color: var(--dark); margin-bottom: 0.75rem; }
    .description p { color: #555; line-height: 1.7; }
    .badge { display: inline-block; padding: 0.25rem 0.75rem; border-radius: 20px; font-size: 0.8rem; color: #fff; }
    .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
    .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
    .btn--warning { background: var(--warning); color: #fff; }
    .btn--danger { background: var(--danger); color: #fff; }
  `]
})
export class EventDetailComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private eventService = inject(EventService);
    private toast = inject(ToastService);
    auth = inject(AuthService);

    event = signal<EventResponse | null>(null);
    loading = signal(true);

    ngOnInit() {
        const id = Number(this.route.snapshot.params['id']);
        this.eventService.getById(id).subscribe({
            next: res => { this.event.set(res.data); this.loading.set(false); },
            error: () => { this.toast.error('Event not found.'); this.router.navigate(['/events']); }
        });
    }

    deleteEvent() {
        if (!confirm('Delete this event?')) return;
        this.eventService.delete(this.event()!.id).subscribe({
            next: () => { this.toast.success('Event deleted.'); this.router.navigate(['/events']); },
            error: err => this.toast.error(err.error?.message || 'Failed to delete.')
        });
    }
}

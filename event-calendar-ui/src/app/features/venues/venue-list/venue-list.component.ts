import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { VenueService } from '../../../core/services/venue.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { VenueResponse } from '../../../core/models/api.model';

@Component({
    selector: 'app-venue-list',
    standalone: true,
    imports: [CommonModule, RouterLink],
    template: `
    <div class="page">
        <div class="page__header">
            <h1>Venues</h1>
            @if (auth.isAdmin()) {
                <a routerLink="/venues/create" class="btn btn--primary">+ Add Venue</a>
            }
        </div>
        @if (loading()) {
            <div class="loading">Loading venues...</div>
        } @else if (venues().length === 0) {
            <div class="empty-state">
                <span>📍</span>
                <p>No venues found.</p>
            </div>
        } @else {
            <div class="venue-grid">
                @for (venue of venues(); track venue.id) {
                    <div class="venue-card" [class.venue-card--inactive]="!venue.isActive">
                        <div class="venue-card__header">
                            <h3>{{ venue.name }}</h3>
                            @if (!venue.isActive) {
                                <span class="inactive-badge">Inactive</span>
                            }
                        </div>
                        <div class="venue-card__meta">
                            <span>📍 {{ venue.city }}, {{ venue.state }}, {{ venue.country }}</span>
                            <span>👥 Capacity: {{ venue.capacity | number }}</span>
                            @if (venue.contactEmail) { <span>✉ {{ venue.contactEmail }}</span> }
                            @if (venue.contactPhone) { <span>📞 {{ venue.contactPhone }}</span> }
                        </div>
                        @if (venue.description) {
                            <p class="venue-card__desc">{{ venue.description }}</p>
                        }
                        @if (auth.isAdmin()) {
                            <div class="venue-card__actions">
                                <a [routerLink]="['/venues', venue.id, 'edit']" class="btn btn--sm btn--warning">Edit</a>
                                <button class="btn btn--sm btn--danger" (click)="deleteVenue(venue.id)">Delete</button>
                            </div>
                        }
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
        .page { padding: 2rem; max-width: 1200px; margin: 0 auto; }
        .page__header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 1.5rem; }
        .page__header h1 { font-size: 1.75rem; color: var(--dark); }
        .loading { text-align: center; padding: 3rem; color: var(--muted); }
        .empty-state { text-align: center; padding: 4rem 2rem; color: var(--muted); display: flex; flex-direction: column; align-items: center; gap: 1rem; }
        .empty-state span { font-size: 3rem; }
        .venue-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 1.5rem; }
        .venue-card { background: var(--card); border-radius: 12px; padding: 1.5rem; box-shadow: 0 2px 12px rgba(0,0,0,0.06); display: flex; flex-direction: column; gap: 0.75rem; }
        .venue-card--inactive { opacity: 0.65; }
        .venue-card__header { display: flex; justify-content: space-between; align-items: flex-start; }
        .venue-card__header h3 { font-size: 1.05rem; font-weight: 700; color: var(--dark); }
        .inactive-badge { background: #fee2e2; color: var(--danger); padding: 0.15rem 0.5rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
        .venue-card__meta { display: flex; flex-direction: column; gap: 0.3rem; font-size: 0.85rem; color: var(--muted); }
        .venue-card__desc { font-size: 0.875rem; color: #555; line-height: 1.5; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }
        .venue-card__actions { display: flex; gap: 0.5rem; margin-top: auto; padding-top: 0.75rem; border-top: 1px solid var(--border); }
        .btn { padding: 0.5rem 1rem; border-radius: 6px; text-decoration: none; font-size: 0.875rem; font-weight: 500; cursor: pointer; border: none; display: inline-block; transition: all 0.2s; }
        .btn--primary { background: var(--primary); color: #fff; }
        .btn--primary:hover { background: var(--primary-dark); text-decoration: none; }
        .btn--outline { background: transparent; border: 1.5px solid var(--border); color: var(--muted); }
        .btn--sm { padding: 0.3rem 0.65rem; font-size: 0.8rem; }
        .btn--warning { background: var(--warning); color: #fff; }
        .btn--danger { background: var(--danger); color: #fff; }
        .btn:disabled { opacity: 0.5; cursor: not-allowed; }
        .pagination { display: flex; justify-content: center; align-items: center; gap: 1rem; margin-top: 2rem; }
    `]
})
export class VenueListComponent implements OnInit {
    private venueService = inject(VenueService);
    private toast = inject(ToastService);
    auth = inject(AuthService);

    venues = signal<VenueResponse[]>([]);
    loading = signal(true);
    page = signal(1);
    totalPages = signal(1);

    ngOnInit() { this.loadVenues(); }

    loadVenues() {
        this.loading.set(true);
        this.venueService.getAll(this.page(), 12).subscribe({
            next: res => {
                this.venues.set(res.data.items);
                this.totalPages.set(res.data.totalPages);
                this.loading.set(false);
            },
            error: () => this.loading.set(false)
        });
    }

    changePage(p: number) { this.page.set(p); this.loadVenues(); }

    deleteVenue(id: number) {
        if (!confirm('Delete this venue?')) return;
        this.venueService.delete(id).subscribe({
            next: () => { this.toast.success('Venue deleted.'); this.loadVenues(); },
            error: err => this.toast.error(err.error?.message || 'Failed to delete venue.')
        });
    }
}

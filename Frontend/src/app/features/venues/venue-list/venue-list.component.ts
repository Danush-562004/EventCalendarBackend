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
    templateUrl: './venue-list.component.html',
    styleUrl: './venue-list.component.scss'
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
            next: res => { this.venues.set(res.data.items); this.totalPages.set(res.data.totalPages); this.loading.set(false); },
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

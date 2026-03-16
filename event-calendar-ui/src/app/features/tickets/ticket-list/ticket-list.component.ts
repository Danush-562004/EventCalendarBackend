import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { ApiResponse, TicketResponse } from '../../../core/models/api.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
    selector: 'app-ticket-list',
    standalone: true,
    imports: [CommonModule],
    template: `
    <div class="page">
      <div class="page__header">
        <h1>My Tickets</h1>
      </div>
      @if (loading()) {
        <div class="loading">Loading tickets...</div>
      } @else if (tickets().length === 0) {
        <div class="empty-state">
          <span>🎫</span>
          <p>No tickets yet. Book tickets for events you're interested in!</p>
        </div>
      } @else {
        <div class="ticket-list">
          @for (ticket of tickets(); track ticket.id) {
            <div class="ticket-card" [class.ticket-card--cancelled]="ticket.status === 'Cancelled'">
              <div class="ticket-card__number">{{ ticket.ticketNumber }}</div>
              <div class="ticket-card__body">
                <h3>{{ ticket.eventTitle }}</h3>
                <div class="ticket-card__meta">
                  <span class="status-badge status-badge--{{ ticket.status.toLowerCase() }}">{{ ticket.status }}</span>
                  <span class="type-badge">{{ ticket.type }}</span>
                  <span>Qty: {{ ticket.quantity }}</span>
                  @if (ticket.seatNumber) { <span>Seat: {{ ticket.seatNumber }}</span> }
                </div>
                <div class="ticket-card__price">₹{{ ticket.price | number:'1.2-2' }}</div>
              </div>
              <div class="ticket-card__checkin">
                @if (ticket.checkedIn) {
                  <span class="checkin-badge">✓ Checked In</span>
                }
              </div>
            </div>
          }
        </div>
      }
    </div>
  `,
    styles: [`
    .page { padding: 2rem; max-width: 900px; margin: 0 auto; }
    .page__header { margin-bottom: 1.5rem; }
    .page__header h1 { font-size: 1.75rem; color: #2c3e50; }
    .loading { text-align: center; padding: 3rem; color: #7f8c8d; }
    .empty-state { text-align: center; padding: 4rem 2rem; color: #7f8c8d; display: flex; flex-direction: column; align-items: center; gap: 1rem; }
    .empty-state span { font-size: 3rem; }
    .ticket-list { display: flex; flex-direction: column; gap: 1rem; }
    .ticket-card { display: flex; align-items: center; gap: 1.5rem; background: #fff; border-radius: 12px; padding: 1.25rem 1.5rem; box-shadow: 0 2px 8px rgba(0,0,0,0.06); border-left: 4px solid #3498db; }
    .ticket-card--cancelled { opacity: 0.6; border-left-color: #e74c3c; }
    .ticket-card__number { font-family: monospace; font-size: 0.8rem; color: #7f8c8d; background: #f0f0f0; padding: 0.4rem 0.75rem; border-radius: 4px; white-space: nowrap; }
    .ticket-card__body { flex: 1; }
    .ticket-card__body h3 { font-size: 1rem; color: #2c3e50; margin-bottom: 0.5rem; }
    .ticket-card__meta { display: flex; align-items: center; gap: 0.75rem; flex-wrap: wrap; margin-bottom: 0.5rem; font-size: 0.85rem; color: #7f8c8d; }
    .status-badge { padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; font-weight: 600; }
    .status-badge--reserved { background: #fff3e0; color: #f39c12; }
    .status-badge--confirmed { background: #e8f5e9; color: #27ae60; }
    .status-badge--cancelled { background: #fce4ec; color: #e74c3c; }
    .status-badge--attended { background: #e3f2fd; color: #1565c0; }
    .type-badge { background: #f0f0f0; padding: 0.2rem 0.6rem; border-radius: 20px; font-size: 0.75rem; }
    .ticket-card__price { font-size: 1.1rem; font-weight: 700; color: #2c3e50; }
    .checkin-badge { background: #e8f5e9; color: #27ae60; padding: 0.3rem 0.75rem; border-radius: 20px; font-size: 0.8rem; font-weight: 600; }
  `]
})
export class TicketListComponent implements OnInit {
    private http = inject(HttpClient);
    private toast = inject(ToastService);

    tickets = signal<TicketResponse[]>([]);
    loading = signal(true);

    ngOnInit() {
        this.http.get<ApiResponse<TicketResponse[]>>(`${environment.apiUrl}/tickets/my-tickets`).subscribe({
            next: res => { this.tickets.set(res.data); this.loading.set(false); },
            error: () => { this.toast.error('Failed to load tickets.'); this.loading.set(false); }
        });
    }
}

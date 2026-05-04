import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { BookingDto } from '../../models/api.models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-my-bookings',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 32px;">🎫 My Bookings</h1>

      <div *ngIf="loading" class="grid"><div class="skeleton" style="height: 120px;" *ngFor="let i of [1,2,3]"></div></div>

      <div *ngIf="!loading && bookings.length === 0" class="card" style="text-align: center; padding: 60px;">
        <span style="font-size: 64px;">📋</span>
        <h3 class="heading-md" style="margin-top: 16px;">No bookings yet</h3>
        <a routerLink="/search" class="btn btn-primary" style="margin-top: 16px;">Search Buses</a>
      </div>

      <div class="bookings-list" *ngIf="!loading">
        <div class="card animate-fadeIn" *ngFor="let b of bookings; let i = index" [style.animation-delay]="(i*0.05)+'s'" style="margin-bottom: 16px;">
          <div class="flex-between" style="margin-bottom: 12px;">
            <div>
              <span class="heading-sm">{{ b.sourceCity }} → {{ b.destinationCity }}</span>
              <p class="text-accent" style="font-weight: 600; font-size: 14px; margin-top: 4px;">{{ b.operatorName }}</p>
              <p class="text-muted" style="font-size: 13px;">Bus: {{ b.busNumber }} &nbsp;|&nbsp; {{ b.departureTime | date:'dd MMM yyyy, hh:mm a' }}</p>
              <div class="flex gap-sm" style="margin-top: 8px; font-size: 12px; color: var(--text-muted);">
                <span>📍 <strong>Pickup:</strong> {{ b.pickupAddress }}</span>
                <span>🏁 <strong>Drop:</strong> {{ b.dropoffAddress }}</span>
              </div>
            </div>
            <div style="text-align: right;">
              <span class="badge" [ngClass]="{
                'badge-success': b.status === 'Paid',
                'badge-warning': b.status === 'Pending',
                'badge-error': b.status === 'Cancelled' || b.status === 'Refunded'
              }">{{ b.status }}</span>
              <p class="heading-sm text-accent" style="margin-top: 4px;">₹{{ b.totalAmount }}</p>
            </div>
          </div>
          <div class="flex gap-sm" style="flex-wrap: wrap;">
            <span class="badge badge-info" *ngFor="let s of b.seats">
              Seat {{ s.seatNumber }}: {{ s.passengerName }}
            </span>
          </div>
          <div class="flex gap-sm" style="margin-top: 12px;">
            <button *ngIf="b.status === 'Paid'" class="btn btn-primary btn-sm" (click)="downloadTicket(b.bookingId)">Download Ticket (PDF)</button>
            <button class="btn btn-secondary btn-sm" (click)="selectedBooking = (selectedBooking === b.bookingId ? null : b.bookingId)">{{ selectedBooking === b.bookingId ? 'Hide Details' : 'View Details' }}</button>
            <button *ngIf="b.status === 'Paid'" class="btn btn-danger btn-sm" (click)="cancelBooking(b)" [id]="'cancel-'+b.bookingId">Cancel</button>
          </div>

          <!-- Booking Details Breakdown -->
          <div *ngIf="selectedBooking === b.bookingId" class="card-flat animate-slideDown" style="margin-top: 16px; border-left: 3px solid var(--accent-primary); padding: 20px;">
            <div class="grid grid-2" style="font-size: 14px; gap: 40px;">
              <div>
                <h4 class="text-muted" style="margin-bottom: 12px; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px;">📍 Journey Info</h4>
                <div style="display: flex; flex-direction: column; gap: 10px;">
                  <div class="flex-between"><span>Operator</span> <strong>{{ b.operatorName }}</strong></div>
                  <div class="flex-between"><span>Pickup Point</span> <span class="text-right" style="max-width: 60%;">{{ b.pickupAddress }}</span></div>
                  <div class="flex-between"><span>Drop-off Point</span> <span class="text-right" style="max-width: 60%;">{{ b.dropoffAddress }}</span></div>
                </div>
                
                <h4 class="text-muted" style="margin-top: 24px; margin-bottom: 12px; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px;">💳 Fare Breakdown</h4>
                <div style="display: flex; flex-direction: column; gap: 8px;">
                  <div class="flex-between"><span>Base Fare</span> <span>₹{{ b.baseFare | number }}</span></div>
                  <div class="flex-between"><span>GST (5%)</span> <span>₹{{ b.gst | number }}</span></div>
                  <div class="flex-between"><span>Service Fee</span> <span>₹{{ b.serviceFee | number }}</span></div>
                  <div class="flex-between"><span>Booking Fee</span> <span>₹{{ b.bookingFee | number }}</span></div>
                  <div *ngIf="b.discountPercent > 0" class="flex-between text-success">
                    <span>Discount ({{ b.discountPercent }}%)</span> 
                    <span>-₹{{ (b.baseFare / (1 - b.discountPercent/100) * (b.discountPercent/100)) | number:'1.0-0' }}</span>
                  </div>
                  <hr style="margin: 8px 0; opacity: 0.05;">
                  <div class="flex-between"><strong class="text-accent" style="font-size: 16px;">Total Paid</strong> <strong class="text-accent" style="font-size: 16px;">₹{{ b.totalAmount | number }}</strong></div>
                </div>
              </div>
              <div style="border-left: 1px solid var(--border-color); padding-left: 24px;">
                <h4 class="text-muted" style="margin-bottom: 12px; font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px;">👥 Passengers</h4>
                <div *ngFor="let s of b.seats" class="card-flat" style="margin-bottom: 10px; padding: 12px; background: var(--bg-surface-light);">
                  <div class="flex-between">
                    <strong>Seat {{ s.seatNumber }}</strong>
                    <span class="badge badge-sm badge-outline">{{ s.passengerGender }}</span>
                  </div>
                  <p style="margin-top: 4px; font-weight: 600;">{{ s.passengerName }}</p>
                  <small class="text-muted">{{ s.passengerAge }} yrs &bull; {{ s.passengerMobile }}</small>
                </div>
              </div>
            </div>
          </div>

          <div *ngIf="b.status === 'Cancelled' && cancelResult[b.bookingId]" class="card-flat" style="margin-top: 12px; padding: 12px; background: var(--bg-surface-light); border-radius: var(--radius-sm);">
            <p class="text-muted">Refund: <strong>{{ cancelResult[b.bookingId].refundPercent }}%</strong> (₹{{ cancelResult[b.bookingId].refundAmount }})</p>
            <p *ngIf="cancelResult[b.bookingId].couponCode" class="text-accent">Coupon: {{ cancelResult[b.bookingId].couponCode }}</p>
          </div>

          <div *ngIf="b.status === 'Refunded'" class="alert alert-warning animate-fadeIn" style="margin-top: 12px;">
            ⚠️ <strong>Emergency Cancellation:</strong> This bus was cancelled by the operator. A full refund has been processed. 
            <div *ngIf="b.cancellationNote" style="margin-top: 8px; padding-top: 8px; border-top: 1px solid rgba(0,0,0,0.1);">
              <strong>Suggested Alternatives:</strong><br>
              <p style="margin-bottom: 8px;">{{ b.cancellationNote }}</p>
              <a [routerLink]="['/search']" [queryParams]="{source: b.sourceCity, destination: b.destinationCity, date: (b.departureTime | date:'yyyy-MM-dd')}" class="btn btn-primary btn-sm">Search Alternative Buses</a>
            </div>
            <p style="margin-top: 8px; font-size: 13px;">Please check your email for a 15% discount coupon for your next booking.</p>
          </div>
        </div>
      </div>
    </div>
  `
})
export class MyBookingsComponent implements OnInit {
  bookings: BookingDto[] = [];
  loading = true;
  selectedBooking: string | null = null;
  cancelResult: Record<string, any> = {};

  constructor(private api: ApiService) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.api.getMyBookings().subscribe({
      next: res => { this.bookings = res.data || []; this.loading = false; },
      error: () => this.loading = false
    });
  }

  cancelBooking(b: BookingDto) {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    this.api.cancelBooking(b.bookingId).subscribe(res => {
      if (res.success && res.data) {
        b.status = 'Cancelled';
        this.cancelResult[b.bookingId] = res.data;
      }
    });
  }

  downloadTicket(bookingId: string) {
    const token = localStorage.getItem('omnibus_token');
    fetch(`${environment.apiUrl}/bookings/${bookingId}/ticket`, {
      headers: { 'Authorization': `Bearer ${token}` }
    })
    .then(res => {
      if (!res.ok) throw new Error('Download failed');
      return res.blob();
    })
    .then(blob => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `OmniBus_Ticket_${bookingId}.pdf`;
      a.click();
      window.URL.revokeObjectURL(url);
    })
    .catch(() => alert('Failed to download ticket. Please try again.'));
  }
}

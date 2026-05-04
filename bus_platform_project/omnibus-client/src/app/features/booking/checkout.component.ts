import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { BookingDto } from '../../models/api.models';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 32px;">💳 Booking Summary</h1>

      <div *ngIf="loading" class="skeleton" style="height: 300px;"></div>

      <div class="grid grid-2" *ngIf="!loading && booking" style="gap: 32px;">
        <!-- Left: Journey Details -->
        <div class="card animate-fadeIn">
          <h2 class="heading-md" style="margin-bottom: 20px;">Journey Details</h2>
          <div class="flex-between" style="margin-bottom: 12px;">
            <span class="text-muted">Route</span>
            <strong>{{ booking.sourceCity }} → {{ booking.destinationCity }}</strong>
          </div>
          <div class="flex-between" style="margin-bottom: 12px;">
            <span class="text-muted">Bus Number</span>
            <strong>{{ booking.busNumber }}</strong>
          </div>
          <div class="flex-between" style="margin-bottom: 24px;">
            <span class="text-muted">Departure</span>
            <strong>{{ booking.departureTime | date:'dd MMM yyyy, hh:mm a' }}</strong>
          </div>

          <h3 class="heading-sm" style="margin-bottom: 12px;">Passengers ({{ booking.seats.length }})</h3>
          <div *ngFor="let s of booking.seats" class="card-flat" style="margin-bottom: 8px; padding: 12px;">
            <strong>Seat {{ s.seatNumber }}: {{ s.passengerName }}</strong>
            <br><small class="text-muted">{{ s.passengerAge }} yrs | {{ s.passengerGender }}</small>
          </div>
        </div>

        <!-- Right: Fare Breakdown -->
        <div class="card animate-fadeIn" style="animation-delay: 0.1s; border-top: 4px solid var(--accent-primary);">
          <h2 class="heading-md" style="margin-bottom: 20px;">Fare Breakdown</h2>
          
          <div class="flex-between" style="margin-bottom: 12px;">
            <span>Base Fare</span>
            <span>₹{{ booking.baseFare | number }}</span>
          </div>
          
          <div class="flex-between" style="margin-bottom: 12px;" *ngIf="booking.discountPercent > 0">
            <span class="text-success">Coupon Discount ({{ booking.discountPercent }}%)</span>
            <span class="text-success">-₹{{ (booking.baseFare / (1 - booking.discountPercent/100) * (booking.discountPercent/100)) | number:'1.0-0' }}</span>
          </div>

          <div class="flex-between" style="margin-bottom: 12px;">
            <span>GST (5%)</span>
            <span>₹{{ booking.gst | number }}</span>
          </div>

          <div class="flex-between" style="margin-bottom: 12px;">
            <span>Service Fee (2%)</span>
            <span>₹{{ booking.serviceFee | number }}</span>
          </div>

          <div class="flex-between" style="margin-bottom: 12px;">
            <span>Booking Fee</span>
            <span>₹{{ booking.bookingFee | number }}</span>
          </div>

          <hr style="margin: 20px 0; opacity: 0.1;">

          <div class="flex-between" style="margin-bottom: 32px;">
            <h3 class="heading-md">Total Amount</h3>
            <h3 class="heading-md text-accent">₹{{ booking.totalAmount | number }}</h3>
          </div>

          <button class="btn btn-primary btn-lg w-100" (click)="proceedToPayment()" id="proceed-btn">
            Confirm & Pay ₹{{ booking.totalAmount | number }}
          </button>
          <p class="text-muted" style="text-align: center; margin-top: 16px; font-size: 12px;">
            By clicking confirm, you agree to our Terms of Service and Cancellation Policy.
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [`.w-100 { width: 100%; }`]
})
export class CheckoutComponent implements OnInit {
  booking: BookingDto | null = null;
  loading = true;

  constructor(private api: ApiService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.api.getBooking(id).subscribe({
        next: r => { this.booking = r.data; this.loading = false; },
        error: () => this.router.navigate(['/search'])
      });
    }
  }

  proceedToPayment() {
    if (this.booking) {
      this.router.navigate(['/payment', this.booking.bookingId]);
    }
  }
}

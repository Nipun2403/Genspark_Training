import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { PassengerDetail } from '../../models/api.models';

@Component({
  selector: 'app-passenger-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 8px;">Passenger Details</h1>
      <p class="text-muted" style="margin-bottom: 32px;">Fill in details for each passenger</p>

      <div class="passengers-grid">
        <div class="card animate-fadeIn" *ngFor="let p of passengers; let i = index" [style.animation-delay]="(i*0.1)+'s'">
          <div class="flex-between" style="margin-bottom: 16px;">
            <h3 class="heading-sm">Seat #{{ lockData.seatNumbers[i] }}</h3>
            <span class="badge badge-info">Passenger {{ i + 1 }}</span>
          </div>
          <div class="grid grid-2">
            <div class="form-group">
              <label class="form-label">Full Name</label>
              <input class="form-input" [(ngModel)]="p.name" placeholder="John Doe" [id]="'pname-'+i">
            </div>
            <div class="form-group">
              <label class="form-label">Age</label>
              <input class="form-input" type="number" [(ngModel)]="p.age" min="1" max="120" [id]="'page-'+i">
            </div>
            <div class="form-group">
              <label class="form-label">Gender</label>
              <select class="form-select" [(ngModel)]="p.gender" [id]="'pgender-'+i">
                <option [ngValue]="0">Male</option>
                <option [ngValue]="1">Female</option>
                <option [ngValue]="2">Other</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Mobile</label>
              <input class="form-input" [(ngModel)]="p.mobile" placeholder="+91 98765 43210" [id]="'pmobile-'+i">
            </div>
          </div>
        </div>
      </div>

      <div class="card" style="margin-top: 24px;">
        <div class="flex-between">
          <div class="form-group" style="flex: 1; max-width: 300px; margin-bottom: 0;">
            <label class="form-label">Coupon Code (Optional)</label>
            <div class="flex gap-sm">
              <input class="form-input" [(ngModel)]="couponCode" placeholder="OMNI-XXXXXXXX" id="coupon-input">
              <button class="btn btn-secondary btn-sm" (click)="validateCoupon()" id="apply-coupon-btn">Apply</button>
            </div>
            <p *ngIf="couponMessage" [class]="couponValid ? 'text-success' : 'text-error'" style="margin-top: 4px; font-size: 12px;">{{ couponMessage }}</p>
          </div>
          <button class="btn btn-primary btn-lg" (click)="createBooking()" [disabled]="loading" id="create-booking-btn">
            {{ loading ? 'Creating...' : 'Proceed to Checkout' }}
          </button>
        </div>
      </div>

      <p *ngIf="error" class="text-error" style="margin-top: 16px;">{{ error }}</p>
    </div>
  `,
  styles: [`.passengers-grid { display: flex; flex-direction: column; gap: 16px; }`]
})
export class PassengerFormComponent implements OnInit {
  passengers: PassengerDetail[] = [];
  lockData: any = { seatNumbers: [], seatIds: [], busId: '' };
  couponCode = '';
  couponValid = false;
  couponMessage = '';
  loading = false;
  error = '';

  constructor(private api: ApiService, private router: Router, private route: ActivatedRoute) {}

  ngOnInit() {
    const stored = sessionStorage.getItem('omnibus_locks');
    if (stored) {
      this.lockData = JSON.parse(stored);
      this.passengers = this.lockData.seatIds.map((seatId: string) => ({
        seatId, name: '', age: 25, gender: 0, mobile: ''
      }));
    }
  }

  validateCoupon() {
    if (!this.couponCode) return;
    this.api.validateCoupon(this.couponCode).subscribe(res => {
      if (res.success && res.data?.isValid) {
        this.couponValid = true;
        this.couponMessage = `✓ ${res.data.discountPercent}% discount applied!`;
      } else {
        this.couponValid = false;
        this.couponMessage = 'Invalid or expired coupon';
      }
    });
  }

  createBooking() {
    const invalid = this.passengers.find(p => !p.name || !p.mobile || p.age < 1);
    if (invalid) { this.error = 'Please fill all passenger details'; return; }
    this.loading = true; this.error = '';
    this.api.createBooking(this.lockData.busId, this.passengers, this.couponValid ? this.couponCode : undefined).subscribe({
      next: res => {
        this.loading = false;
        if (res.success && res.data) {
          this.router.navigate(['/checkout', res.data.bookingId]);
        } else { this.error = res.message; }
      },
      error: () => { this.loading = false; this.error = 'Failed to create booking'; }
    });
  }
}

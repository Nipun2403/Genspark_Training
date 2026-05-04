import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-payment-sim',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-content flex-center" style="min-height: calc(100vh - 72px);">
      <div class="payment-card card animate-fadeIn" *ngIf="!processed">
        <div style="text-align: center; margin-bottom: 32px;">
          <span style="font-size: 64px;">💳</span>
          <h2 class="heading-lg" style="margin-top: 16px;">Simulated Payment Gateway</h2>
          <p class="text-muted">This is a test gateway. Choose an outcome below.</p>
        </div>
        <div class="flex gap-md" style="justify-content: center;">
          <button class="btn btn-success btn-lg" (click)="processPayment(true)" [disabled]="processing" id="pay-success-btn">
            ✅ Pay Successfully
          </button>
          <button class="btn btn-danger btn-lg" (click)="processPayment(false)" [disabled]="processing" id="pay-fail-btn">
            ❌ Simulate Failure
          </button>
        </div>
        <p *ngIf="processing" class="text-accent" style="text-align: center; margin-top: 16px;">Processing...</p>
      </div>

      <div class="result-card card animate-fadeIn" *ngIf="processed">
        <div style="text-align: center;">
          <span style="font-size: 80px;">{{ success ? '🎉' : '😞' }}</span>
          <h2 class="heading-lg" [class]="success ? 'text-success' : 'text-error'" style="margin-top: 16px;">
            {{ success ? 'Payment Successful!' : 'Payment Failed' }}
          </h2>
          <p class="text-muted" style="margin: 8px 0 24px;">{{ message }}</p>
          <p *ngIf="paymentRef" style="margin-bottom: 24px;">
            Payment Ref: <strong class="text-accent">{{ paymentRef }}</strong>
          </p>
          <div class="flex gap-md" style="justify-content: center;">
            <a routerLink="/my-bookings" class="btn btn-primary" *ngIf="success" id="view-bookings-btn">View My Bookings</a>
            <a routerLink="/" class="btn btn-secondary" id="go-home-btn">Go Home</a>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .payment-card, .result-card { max-width: 520px; width: 100%; padding: 48px; }
  `]
})
export class PaymentSimComponent implements OnInit {
  bookingId = '';
  processing = false;
  processed = false;
  success = false;
  message = '';
  paymentRef: string | null = null;

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit() { this.bookingId = this.route.snapshot.paramMap.get('bookingId')!; }

  processPayment(isSuccess: boolean) {
    this.processing = true;
    this.api.processPayment(this.bookingId, isSuccess).subscribe({
      next: res => {
        this.processing = false;
        this.processed = true;
        this.success = res.data?.success ?? false;
        this.message = res.data?.message ?? res.message;
        this.paymentRef = res.data?.paymentRef ?? null;
      },
      error: () => {
        this.processing = false;
        this.processed = true;
        this.success = false;
        this.message = 'Payment processing error';
      }
    });
  }
}

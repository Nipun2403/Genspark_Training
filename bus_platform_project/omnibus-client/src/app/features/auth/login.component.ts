import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-content flex-center" style="min-height: calc(100vh - 72px);">
      <div class="login-card card animate-fadeIn" id="login-card">
        <div class="login-header">
          <span class="login-icon">🚌</span>
          <h2 class="heading-lg">Welcome to OmniBus</h2>
          <p class="text-muted">Login with your email — no password needed!</p>
        </div>

        <div *ngIf="!otpSent" class="login-form">
          <div class="form-group">
            <label class="form-label">Email Address</label>
            <input class="form-input" type="email" [(ngModel)]="email" placeholder="you@example.com" id="login-email"
              (keyup.enter)="sendOtp()">
          </div>
          <button class="btn btn-primary btn-lg" style="width:100%" (click)="sendOtp()" [disabled]="loading" id="send-otp-btn">
            {{ loading ? 'Sending...' : '✉️ Send OTP' }}
          </button>
        </div>

        <div *ngIf="otpSent" class="otp-form animate-fadeIn">
          <p style="margin-bottom: 16px;">OTP sent to <strong class="text-accent">{{ email }}</strong></p>
          <div class="otp-inputs">
            <input *ngFor="let d of [0,1,2,3,4,5]" class="otp-digit" maxlength="1" [(ngModel)]="digits[d]"
              (input)="onDigitInput($event, d)" (keydown.backspace)="onBackspace($event, d)" [id]="'otp-' + d">
          </div>
          <button class="btn btn-primary btn-lg" style="width:100%; margin-top: 20px;" (click)="verifyOtp()" [disabled]="loading" id="verify-otp-btn">
            {{ loading ? 'Verifying...' : '🔓 Verify & Login' }}
          </button>
          <button class="btn btn-secondary btn-sm" style="width:100%; margin-top: 8px;" (click)="otpSent = false" id="back-btn">
            ← Back
          </button>
        </div>

        <p *ngIf="error" class="text-error" style="margin-top: 16px; text-align: center;">{{ error }}</p>
        <p *ngIf="successMsg" class="text-success" style="margin-top: 16px; text-align: center;">{{ successMsg }}</p>
      </div>
    </div>
  `,
  styles: [`
    .login-card { max-width: 420px; width: 100%; padding: 40px; }
    .login-header { text-align: center; margin-bottom: 32px; }
    .login-icon { font-size: 48px; display: block; margin-bottom: 16px; }
    .otp-inputs { display: flex; gap: 10px; justify-content: center; }
    .otp-digit {
      width: 48px; height: 56px; text-align: center; font-size: 24px; font-weight: 700;
      background: var(--bg-surface-light); border: 2px solid var(--border-color);
      border-radius: var(--radius-sm); color: var(--accent-primary);
      transition: var(--transition);
      &:focus { border-color: var(--accent-primary); outline: none; box-shadow: 0 0 0 3px rgba(187,134,252,0.2); }
    }
  `]
})
export class LoginComponent {
  email = '';
  digits = ['', '', '', '', '', ''];
  otpSent = false;
  loading = false;
  error = '';
  successMsg = '';

  constructor(private auth: AuthService, private router: Router) {}

  sendOtp() {
    if (!this.email) { this.error = 'Please enter your email'; return; }
    this.loading = true; this.error = '';
    this.auth.sendOtp(this.email).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success) { this.otpSent = true; this.successMsg = 'OTP sent! Check your email.'; }
        else { this.error = res.message; }
      },
      error: () => { this.loading = false; this.error = 'Failed to send OTP'; }
    });
  }

  verifyOtp() {
    const code = this.digits.join('');
    if (code.length !== 6) { this.error = 'Please enter all 6 digits'; return; }
    this.loading = true; this.error = '';
    this.auth.verifyOtp(this.email, code).subscribe({
      next: (res) => {
        this.loading = false;
        if (res.success && res.data) {
          const role = res.data.role;
          if (role === 'Admin') this.router.navigate(['/admin']);
          else if (role === 'Operator') this.router.navigate(['/operator/dashboard']);
          else this.router.navigate(['/']);
        }
        else { this.error = res.message; }
      },
      error: () => { this.loading = false; this.error = 'Verification failed'; }
    });
  }

  onDigitInput(event: Event, index: number) {
    const input = event.target as HTMLInputElement;
    if (input.value && index < 5) {
      const next = document.getElementById('otp-' + (index + 1)) as HTMLInputElement;
      next?.focus();
    }
  }

  onBackspace(event: Event, index: number) {
    if (!this.digits[index] && index > 0) {
      const prev = document.getElementById('otp-' + (index - 1)) as HTMLInputElement;
      prev?.focus();
    }
  }
}

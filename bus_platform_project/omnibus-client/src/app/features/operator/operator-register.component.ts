import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-operator-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-content container" style="max-width: 640px;">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 8px;">🚌 Become an Operator</h1>
      <p class="text-muted" style="margin-bottom: 32px;">Register your fleet and start earning.</p>

      <div class="card animate-fadeIn">
        <div class="form-group">
          <label class="form-label">Full Name</label>
          <input class="form-input" [(ngModel)]="data.fullName" placeholder="Your full name" id="op-name">
        </div>
        <div class="form-group">
          <label class="form-label">Phone</label>
          <input class="form-input" [(ngModel)]="data.phone" placeholder="+91 98765 43210" id="op-phone">
        </div>
        <div class="form-group">
          <label class="form-label">Business Name</label>
          <input class="form-input" [(ngModel)]="data.businessName" placeholder="Your bus company name" id="op-business">
        </div>
        <div class="form-group">
          <label class="form-label">Contact Details</label>
          <textarea class="form-input" [(ngModel)]="data.contactDetails" rows="3" placeholder="Office address, email, etc." id="op-contact" style="resize: vertical;"></textarea>
        </div>
        <button class="btn btn-primary btn-lg" style="width: 100%;" (click)="register()" [disabled]="loading" id="register-operator-btn">
          {{ loading ? 'Submitting...' : '📋 Submit Registration' }}
        </button>
        <p *ngIf="error" class="text-error" style="margin-top: 12px;">{{ error }}</p>
        <p *ngIf="successMsg" class="text-success" style="margin-top: 12px;">{{ successMsg }}</p>
      </div>
    </div>
  `
})
export class OperatorRegisterComponent {
  data = { fullName: '', phone: '', businessName: '', contactDetails: '' };
  loading = false; error = ''; successMsg = '';

  constructor(private api: ApiService, private router: Router) {}

  register() {
    if (!this.data.fullName || !this.data.businessName) { this.error = 'Please fill required fields'; return; }
    this.loading = true; this.error = '';
    this.api.registerOperator(this.data).subscribe({
      next: res => {
        this.loading = false;
        if (res.success) this.successMsg = 'Registration submitted! Awaiting admin approval.';
        else this.error = res.message;
      },
      error: () => { this.loading = false; this.error = 'Registration failed'; }
    });
  }
}

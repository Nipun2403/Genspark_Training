import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="hero">
      <div class="container">
        <div class="hero-content animate-fadeIn">
          <h1 class="hero-title">Travel Smarter.<br><span class="text-accent">Book Faster.</span></h1>
          <p class="hero-subtitle">India's most reliable bus booking platform with real-time seat availability and zero overbooking guarantee.</p>
          <div class="search-box card">
            <div class="search-fields">
              <div class="field">
                <label class="form-label">From</label>
                <input class="form-input" [(ngModel)]="source" (input)="onTypeSource()" placeholder="e.g., Delhi" id="search-from" autocomplete="off">
                <div class="suggestions" *ngIf="sourceSuggestions.length > 0">
                  <div class="suggestion" *ngFor="let s of sourceSuggestions" (click)="selectSource(s)">{{ s }}</div>
                </div>
              </div>
              <button class="swap-btn" (click)="swapCities()" id="swap-cities-btn">⇄</button>
              <div class="field">
                <label class="form-label">To</label>
                <input class="form-input" [(ngModel)]="destination" (input)="onTypeDest()" placeholder="e.g., Mumbai" id="search-to" autocomplete="off">
                <div class="suggestions" *ngIf="destSuggestions.length > 0">
                  <div class="suggestion" *ngFor="let s of destSuggestions" (click)="selectDest(s)">{{ s }}</div>
                </div>
              </div>
              <div class="field">
                <label class="form-label">Date</label>
                <input class="form-input" type="date" [(ngModel)]="date" id="search-date">
              </div>
              <div class="field">
                <label class="form-label">Trip</label>
                <select class="form-select" [(ngModel)]="tripType" id="trip-type">
                  <option value="oneway">One Way</option>
                  <option value="round">Round Trip</option>
                </select>
              </div>
              <button class="btn btn-primary btn-lg search-btn" (click)="search()" id="search-btn">
                🔍 Search Buses
              </button>
            </div>
          </div>
        </div>
          <div class="features grid grid-3">
            <div class="feature-card card animate-fadeIn" *ngFor="let f of features; let i = index" [style.animation-delay]="(i * 0.1) + 's'">
              <span class="feature-icon">{{ f.icon }}</span>
              <h3 class="feature-title">{{ f.title }}</h3>
              <p class="feature-desc">{{ f.desc }}</p>
            </div>
          </div>
      </div>
    </section>
  `,
  styles: [`
    .hero { padding: 60px 0 100px; }
    .hero-title { font-size: 48px; font-weight: 800; line-height: 1.1; letter-spacing: -1px; margin-bottom: 16px; }
    .hero-subtitle { font-size: 18px; color: var(--text-secondary); max-width: 540px; margin-bottom: 40px; }
    .search-box { padding: 32px; margin-bottom: 80px; position: relative; z-index: 1000; }
    .search-fields { display: flex; gap: 16px; align-items: flex-end; flex-wrap: wrap; }
    .field { flex: 1; min-width: 160px; position: relative; }
    .swap-btn {
      width: 44px; height: 44px; border-radius: 50%; background: var(--bg-surface-light);
      border: 1px solid var(--border-color); color: var(--accent-primary); font-size: 18px;
      cursor: pointer; transition: var(--transition); margin-bottom: 0; align-self: flex-end;
      &:hover { background: var(--accent-primary); color: #121212; transform: rotate(180deg); }
    }
    .search-btn { min-width: 180px; height: 48px; }
    .suggestions {
      position: absolute; top: 100%; left: 0; right: 0; background: var(--bg-surface);
      border: 1px solid var(--border-color); border-radius: 0 0 8px 8px; z-index: 9999;
      box-shadow: 0 12px 32px rgba(0,0,0,0.4); max-height: 240px; overflow-y: auto;
    }
    .suggestion { padding: 10px 16px; cursor: pointer; transition: background 0.2s; &:hover { background: var(--bg-surface-light); color: var(--accent-primary); } }
    
    .features { margin-top: 140px; z-index: 1; position: relative; }
    .feature-card { text-align: center; padding: 16px; transform: scale(0.85); background: rgba(255,255,255,0.02); }
    .feature-icon { font-size: 28px; display: block; margin-bottom: 8px; }
    .feature-title { font-size: 15px; font-weight: 700; margin-bottom: 4px; }
    .feature-desc { font-size: 12px; color: var(--text-muted); line-height: 1.3; }

    @media (max-width: 768px) {
      .hero-title { font-size: 32px; }
      .search-fields { flex-direction: column; }
      .swap-btn { align-self: center; }
    }
  `]
})
export class HomeComponent implements OnInit {
  source = '';
  destination = '';
  date = '';
  tripType = 'oneway';
  sourceSuggestions: string[] = [];
  destSuggestions: string[] = [];
  private sourceTimer: any;
  private destTimer: any;

  features = [
    { icon: '🔒', title: 'Zero Overbooking', desc: 'Concurrency-safe seat locking ensures no double bookings.' },
    { icon: '⚡', title: 'Instant Booking', desc: 'Lock seats in under 200ms with our reservation engine.' },
    { icon: '🎫', title: 'Digital Tickets', desc: 'PDF tickets with QR codes delivered instantly.' }
  ];

  constructor(private router: Router, private api: ApiService, private auth: AuthService) {}

  ngOnInit() {
    if (this.auth.isLoggedIn) {
      if (this.auth.role === 'Admin') this.router.navigate(['/admin']);
      else if (this.auth.role === 'Operator') this.router.navigate(['/operator/dashboard']);
    }
  }

  swapCities() {
    [this.source, this.destination] = [this.destination, this.source];
  }

  search() {
    const params: any = {};
    if (this.source) params.source = this.source;
    if (this.destination) params.destination = this.destination;
    if (this.date) params.date = this.date;
    this.router.navigate(['/search'], { queryParams: params });
  }

  onTypeSource() {
    clearTimeout(this.sourceTimer);
    if (!this.source) { this.sourceSuggestions = []; return; }
    this.sourceTimer = setTimeout(() => {
      // If destination is selected, only suggest sources to that destination
      this.api.getCitySuggestions(this.source, undefined, this.destination).subscribe(r => this.sourceSuggestions = r.data || []);
    }, 300);
  }

  onTypeDest() {
    clearTimeout(this.destTimer);
    if (!this.destination) { this.destSuggestions = []; return; }
    this.destTimer = setTimeout(() => {
      // If source is selected, only suggest destinations from that source
      this.api.getCitySuggestions(this.destination, this.source).subscribe(r => this.destSuggestions = r.data || []);
    }, 300);
  }

  selectSource(s: string) { this.source = s; this.sourceSuggestions = []; }
  selectDest(s: string) { this.destination = s; this.destSuggestions = []; }
}

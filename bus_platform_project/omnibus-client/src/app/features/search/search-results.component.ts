import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { SearchResultDto } from '../../models/api.models';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 8px;">🔍 Search Results</h1>
      <p class="text-muted animate-fadeIn" style="margin-bottom: 32px;" *ngIf="source || destination">
        {{ source || 'Any' }} → {{ destination || 'Any' }}
        <span *ngIf="date"> &nbsp;|&nbsp; {{ date }}</span>
      </p>

      <div *ngIf="loading" class="grid grid-2">
        <div class="skeleton" style="height: 200px;" *ngFor="let i of [1,2,3,4]"></div>
      </div>

      <div *ngIf="!loading && results.length === 0" class="empty-state card animate-fadeIn" style="text-align: center; padding: 60px;">
        <span style="font-size: 64px;">🚌</span>
        <h3 class="heading-md" style="margin-top: 16px;">No buses found</h3>
        <p class="text-muted">Try different cities or dates</p>
      </div>

      <div class="grid grid-2" *ngIf="!loading && results.length > 0">
        <div class="bus-card card animate-fadeIn" *ngFor="let bus of results; let i = index" [style.animation-delay]="(i * 0.05) + 's'">
          <div class="flex-between" style="margin-bottom: 16px;">
            <div>
              <span class="heading-sm">{{ bus.busNumber }}</span>
              <p class="text-muted" style="font-size: 13px;">by {{ bus.operatorName }}</p>
            </div>
            <div class="price">₹{{ bus.basePrice }}</div>
          </div>
          <div class="route-line">
            <div class="route-point">
              <span class="dot dot-green"></span>
              <div><strong>{{ bus.sourceCity }}</strong><p class="text-muted" style="font-size: 12px;">{{ bus.pickupAddress }}</p></div>
            </div>
            <div class="route-dash"></div>
            <div class="route-point">
              <span class="dot dot-red"></span>
              <div><strong>{{ bus.destinationCity }}</strong><p class="text-muted" style="font-size: 12px;">{{ bus.dropoffAddress }}</p></div>
            </div>
          </div>
          <div class="flex-between" style="margin-top: 16px;">
            <div class="flex gap-sm" style="flex-wrap: wrap;">
              <span class="badge badge-outline">📅 {{ bus.departureTime | date:'dd MMM' }}</span>
              <span class="badge badge-info">🕐 {{ bus.departureTime | date:'shortTime' }}</span>
              <span class="badge" [ngClass]="bus.availableSeats > 10 ? 'badge-success' : bus.availableSeats > 0 ? 'badge-warning' : 'badge-error'">
                {{ bus.availableSeats }}/{{ bus.totalSeats }} seats
              </span>
            </div>
            <a [routerLink]="['/bus', bus.busId, 'seats']" class="btn btn-primary btn-sm" *ngIf="bus.availableSeats > 0" [id]="'select-bus-' + bus.busId">
              Select Seats →
            </a>
            <span *ngIf="bus.availableSeats === 0" class="badge badge-error">Fully Booked</span>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .price { font-size: 24px; font-weight: 800; color: var(--accent-primary); }
    .route-line { display: flex; align-items: center; gap: 12px; padding: 12px 0; }
    .route-point { display: flex; align-items: flex-start; gap: 8px; flex: 1; }
    .route-dash { flex: 0 0 40px; height: 2px; background: linear-gradient(90deg, var(--success), var(--error)); border-radius: 1px; }
    .dot { width: 10px; height: 10px; border-radius: 50%; margin-top: 5px; flex-shrink: 0; }
    .dot-green { background: var(--success); }
    .dot-red { background: var(--error); }
    .bus-card { cursor: default; }
  `]
})
export class SearchResultsComponent implements OnInit {
  results: SearchResultDto[] = [];
  loading = true;
  source = '';
  destination = '';
  date = '';

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router, private auth: AuthService) {}

  ngOnInit() {
    if (this.auth.isLoggedIn) {
      if (this.auth.role === 'Admin') { this.router.navigate(['/admin']); return; }
      if (this.auth.role === 'Operator') { this.router.navigate(['/operator/dashboard']); return; }
    }

    this.route.queryParams.subscribe(params => {
      this.source = params['source'] || '';
      this.destination = params['destination'] || '';
      this.date = params['date'] || '';
      this.loadResults();
    });
  }

  loadResults() {
    this.loading = true;
    this.api.searchBuses(this.source, this.destination, this.date).subscribe({
      next: res => { this.results = res.data || []; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}

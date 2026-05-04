import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { SeatDto } from '../../models/api.models';
import { interval, Subscription } from 'rxjs';

@Component({
  selector: 'app-seat-map',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-content container">
      <div class="flex-between" style="margin-bottom: 32px;">
        <div class="animate-fadeIn">
          <h1 class="heading-xl">Select Your Seats</h1>
          <p class="text-muted">Choose up to 5 seats. Selected seats are locked for 5 minutes.</p>
        </div>
        <div *ngIf="lockExpiry" class="timer-box card animate-fadeIn">
          <span class="text-muted">Lock expires in</span>
          <span class="timer text-accent">{{ timerDisplay }}</span>
        </div>
      </div>

      <div class="seat-layout animate-fadeIn">
        <div class="legend flex gap-md" style="margin-bottom: 24px;">
          <span class="flex gap-sm"><span class="seat-demo seat-available"></span> Available</span>
          <span class="flex gap-sm"><span class="seat-demo seat-selected"></span> Selected</span>
          <span class="flex gap-sm"><span class="seat-demo seat-locked"></span> Locked</span>
          <span class="flex gap-sm"><span class="seat-demo seat-booked"></span> Booked</span>
        </div>

        <div class="bus-frame">
          <div class="driver-area">🧑‍✈️ Driver</div>
          <div class="seat-grid">
            <div *ngFor="let seat of seats; let i = index" class="seat"
              [ngClass]="getSeatClass(seat)"
              (click)="toggleSeat(seat)"
              [style.grid-column]="(i % 4 < 2) ? (i % 4) + 1 : (i % 4) + 2"
              [id]="'seat-' + seat.seatNumber">
              {{ seat.seatNumber }}
            </div>
          </div>
        </div>

        <div class="action-bar flex-between" style="margin-top: 32px;">
          <div>
            <span class="text-muted">Selected: </span>
            <span class="text-accent heading-sm">{{ selectedSeats.length }} seat(s)</span>
          </div>
          <button class="btn btn-primary btn-lg" [disabled]="selectedSeats.length === 0 || locking"
            (click)="lockAndProceed()" id="lock-seats-btn">
            {{ locking ? 'Locking...' : '🔒 Lock & Continue' }}
          </button>
        </div>
      </div>

      <p *ngIf="error" class="text-error" style="margin-top: 16px;">{{ error }}</p>
    </div>
  `,
  styles: [`
    .timer-box { padding: 12px 20px; text-align: center; }
    .timer { font-size: 28px; font-weight: 800; display: block; }
    .seat-demo { width: 20px; height: 20px; border-radius: 6px; display: inline-block; }
    .bus-frame {
      background: var(--bg-surface); border: 2px solid var(--border-color);
      border-radius: var(--radius-lg); padding: 24px; max-width: 560px; margin: 0 auto;
    }
    .driver-area {
      text-align: right; padding: 12px; margin-bottom: 16px;
      border-bottom: 2px dashed var(--border-color); font-size: 14px; color: var(--text-muted);
    }
    .seat-grid {
      display: grid;
      grid-template-columns: 56px 56px 60px 56px 56px;
      column-gap: 12px;
      row-gap: 12px;
      justify-content: center;
      margin: 0 auto;
    }
    .seat {
      width: 56px; height: 56px; border-radius: 10px; display: flex; align-items: center;
      justify-content: center; font-weight: 700; font-size: 14px; cursor: pointer;
      transition: all 0.2s ease; border: 2px solid transparent;
    }
    .seat-available { background: var(--bg-surface-light); color: var(--text-primary); border-color: var(--border-color);
      &:hover { border-color: var(--accent-primary); transform: scale(1.08); }
    }
    .seat-selected { background: var(--accent-primary); color: #121212; border-color: var(--accent-primary);
      transform: scale(1.05); box-shadow: 0 0 12px rgba(187,134,252,0.4);
    }
    .seat-locked { background: var(--bg-surface-light); color: var(--warning); border-color: var(--warning); cursor: not-allowed; opacity: 0.7; }
    .seat-booked { background: var(--bg-surface-light); color: var(--error); border-color: var(--error); cursor: not-allowed; opacity: 0.5; }
  `]
})
export class SeatMapComponent implements OnInit, OnDestroy {
  busId = '';
  seats: SeatDto[] = [];
  selectedSeats: SeatDto[] = [];
  lockExpiry: Date | null = null;
  timerDisplay = '05:00';
  error = '';
  locking = false;
  private timerSub?: Subscription;

  constructor(private api: ApiService, private route: ActivatedRoute, private router: Router) {}

  ngOnInit() {
    this.busId = this.route.snapshot.paramMap.get('busId')!;
    this.loadSeatMap();
  }

  ngOnDestroy() { this.timerSub?.unsubscribe(); }

  loadSeatMap() {
    this.api.getSeatMap(this.busId).subscribe(res => {
      if (res.success) this.seats = res.data || [];
    });
  }

  getSeatClass(seat: SeatDto): string {
    if (this.selectedSeats.find(s => s.seatId === seat.seatId)) return 'seat-selected';
    if (seat.status === 'Locked') return 'seat-locked';
    if (seat.status === 'Booked') return 'seat-booked';
    return 'seat-available';
  }

  toggleSeat(seat: SeatDto) {
    if (seat.status !== 'Available') return;
    const idx = this.selectedSeats.findIndex(s => s.seatId === seat.seatId);
    if (idx >= 0) { this.selectedSeats.splice(idx, 1); }
    else {
      if (this.selectedSeats.length >= 5) { this.error = 'Maximum 5 seats per transaction'; return; }
      this.selectedSeats.push(seat);
      this.error = '';
    }
  }

  lockAndProceed() {
    this.locking = true; this.error = '';
    const seatNumbers = this.selectedSeats.map(s => s.seatNumber);
    this.api.lockSeats(this.busId, seatNumbers).subscribe({
      next: res => {
        this.locking = false;
        if (res.success && res.data) {
          this.lockExpiry = new Date(res.data.expiresAt);
          this.startTimer();
          // Store lock info and navigate
          sessionStorage.setItem('omnibus_locks', JSON.stringify({
            busId: this.busId, lockIds: res.data.lockIds,
            seatIds: this.selectedSeats.map(s => s.seatId),
            seatNumbers: seatNumbers, expiresAt: res.data.expiresAt
          }));
          this.router.navigate(['/booking', 'new', 'passengers'], { queryParams: { busId: this.busId } });
        } else { this.error = res.message; }
      },
      error: () => { this.locking = false; this.error = 'Failed to lock seats. Try again.'; }
    });
  }

  startTimer() {
    this.timerSub = interval(1000).subscribe(() => {
      if (!this.lockExpiry) return;
      const diff = this.lockExpiry.getTime() - Date.now();
      if (diff <= 0) { this.timerDisplay = '00:00'; this.timerSub?.unsubscribe(); this.loadSeatMap(); return; }
      const m = Math.floor(diff / 60000); const s = Math.floor((diff % 60000) / 1000);
      this.timerDisplay = `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`;
    });
  }
}

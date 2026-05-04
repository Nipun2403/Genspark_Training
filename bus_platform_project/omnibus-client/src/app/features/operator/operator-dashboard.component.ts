import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { BusDto, RouteDto, BusScheduleDto } from '../../models/api.models';

@Component({
  selector: 'app-operator-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 32px;">🚌 Operator Dashboard</h1>

      <div class="grid grid-4" style="margin-bottom: 32px;">
        <div class="card stat-card animate-fadeIn clickable" [class.active-tab-card]="tab==='revenue'" (click)="tab='revenue'">
          <span class="stat-label">Net Earning</span>
          <span class="stat-value text-accent">₹{{ revenue?.totalEarnings || 0 | number }}</span>
        </div>
        <div class="card stat-card animate-fadeIn clickable" [class.active-tab-card]="tab=='fleet'" (click)="tab='fleet'" style="animation-delay: 0.1s;">
          <span class="stat-label">Active Fleet</span>
          <span class="stat-value">{{ buses.length }}</span>
        </div>
        <div class="card stat-card animate-fadeIn clickable" [class.active-tab-card]="tab=='schedules'" (click)="tab='schedules'" style="animation-delay: 0.2s;">
          <span class="stat-label">Recurring Schedules</span>
          <span class="stat-value text-primary">{{ schedules.length }}</span>
        </div>
        <div class="card stat-card animate-fadeIn" style="animation-delay: 0.3s;">
          <span class="stat-label">Account Status</span>
          <span class="badge" [ngClass]="{'badge-success': profile?.approvalStatus === 'Approved', 'badge-warning': profile?.approvalStatus === 'Pending', 'badge-error': profile?.approvalStatus === 'Disabled' || profile?.approvalStatus === 'Rejected'}" style="font-size: 14px; margin-top: 8px;">
            {{ profile?.approvalStatus || 'Loading...' }}
          </span>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs" style="margin-bottom: 24px;">
        <button class="tab" [class.active]="tab==='fleet'" (click)="tab='fleet'">🚌 My Fleet</button>
        <button class="tab" [class.active]="tab==='schedules'" (click)="tab='schedules'">⏰ Schedules</button>
        <button class="tab" [class.active]="tab==='add'" (click)="tab='add'">➕ Add Bus</button>
        <button class="tab" [class.active]="tab==='revenue'" (click)="tab='revenue'">📈 Revenue</button>
      </div>

      <!-- Add Bus Tab -->
      <div *ngIf="tab === 'add'" class="animate-fadeIn">
        <div class="card" style="margin-bottom: 32px;">
          <h2 class="heading-md" style="margin-bottom: 16px;">{{ editingBusId ? '📝 Edit Bus Instance' : '➕ Add One-Time Bus Trip' }}</h2>
          <div class="grid grid-3">
            <div class="form-group">
              <label class="form-label">Route</label>
              <select class="form-select" [(ngModel)]="newBus.routeId">
                <option value="">Select route</option>
                <option *ngFor="let r of routes" [value]="r.routeId">{{ r.sourceCity }} → {{ r.destinationCity }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Plate Number</label>
              <input class="form-input" [(ngModel)]="newBus.plateNumber" placeholder="XX-00-XX-0000">
            </div>
            <div class="form-group">
              <label class="form-label">Bus Number</label>
              <input class="form-input" [(ngModel)]="newBus.busNumber" placeholder="BUS-001">
            </div>
            <div class="form-group">
              <label class="form-label">Base Price (₹)</label>
              <input class="form-input" type="number" [(ngModel)]="newBus.basePrice">
            </div>
            <div class="form-group">
              <label class="form-label">Departure</label>
              <input class="form-input" type="datetime-local" [(ngModel)]="newBus.departureTime">
            </div>
            <div class="form-group">
              <label class="form-label">Total Seats</label>
              <input class="form-input" type="number" [(ngModel)]="newBus.totalSeats">
            </div>
            <div class="form-group">
              <label class="form-label">Pickup Address</label>
              <input class="form-input" [(ngModel)]="newBus.pickupAddress" placeholder="Location">
            </div>
            <div class="form-group">
              <label class="form-label">Drop-off Address</label>
              <input class="form-input" [(ngModel)]="newBus.dropoffAddress" placeholder="Location">
            </div>
          </div>
          <div class="flex gap-sm">
            <button class="btn btn-primary" (click)="editingBusId ? saveBus() : addBus()" [disabled]="processing">
              {{ processing ? 'Processing...' : (editingBusId ? 'Save Changes' : 'Add One-Time Trip') }}
            </button>
            <button *ngIf="editingBusId" class="btn btn-secondary" (click)="cancelEdit()">Cancel</button>
          </div>
          <p *ngIf="errorMsg" class="text-error" style="margin-top: 8px;">{{ errorMsg }}</p>
        </div>
      </div>

      <!-- Schedules Tab -->
      <div *ngIf="tab === 'schedules'" class="animate-fadeIn">
        <div class="card" style="margin-bottom: 32px; border: 1px dashed var(--accent-primary);">
          <h3 class="heading-md" style="margin-bottom: 16px;">➕ Create Recurring Daily Schedule</h3>
          <p class="text-muted" style="margin-bottom: 16px; font-size: 14px;">Buses will be automatically created every day at midnight for tomorrow's trips.</p>
          <div class="grid grid-3">
             <div class="form-group">
              <label class="form-label">Route</label>
              <select class="form-select" [(ngModel)]="newSchedule.routeId">
                <option value="">Select route</option>
                <option *ngFor="let r of routes" [value]="r.routeId">{{ r.sourceCity }} → {{ r.destinationCity }}</option>
              </select>
            </div>
            <div class="form-group">
              <label class="form-label">Daily Departure Time</label>
              <input class="form-input" type="time" [(ngModel)]="newSchedule.departureTime">
            </div>
            <div class="form-group">
              <label class="form-label">Bus Number / Service Name</label>
              <input class="form-input" [(ngModel)]="newSchedule.busNumber" placeholder="Ex: Morning Express">
            </div>
            <div class="form-group">
              <label class="form-label">Base Price (₹)</label>
              <input class="form-input" type="number" [(ngModel)]="newSchedule.basePrice">
            </div>
            <div class="form-group">
              <label class="form-label">Total Seats</label>
              <input class="form-input" type="number" [(ngModel)]="newSchedule.totalSeats">
            </div>
            <div class="form-group">
              <label class="form-label">Plate Number</label>
              <input class="form-input" [(ngModel)]="newSchedule.plateNumber">
            </div>
          </div>
          <button class="btn btn-primary" (click)="addSchedule()" [disabled]="processing">
            {{ processing ? 'Creating...' : '🚀 Start Recurring Service' }}
          </button>
          <p *ngIf="errorMsg" class="text-error" style="margin-top: 8px;">{{ errorMsg }}</p>
        </div>

        <h3 class="heading-md" style="margin-bottom: 16px;">Active Schedules</h3>
        <div class="table-container">
          <table>
            <thead><tr><th>Bus/Service</th><th>Route</th><th>Time (Daily)</th><th>Seats</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>
              <tr *ngFor="let s of schedules">
                <td><strong>{{ s.busNumber }}</strong><br><small>{{ s.plateNumber }}</small></td>
                <td>{{ s.sourceCity }} → {{ s.destinationCity }}</td>
                <td><span class="badge badge-info">{{ s.departureTime }}</span></td>
                <td>{{ s.totalSeats }}</td>
                <td>
                  <span class="badge" [ngClass]="s.isActive ? 'badge-success' : 'badge-warning'">
                    {{ s.isActive ? 'Running' : 'Paused' }}
                  </span>
                </td>
                <td class="flex gap-sm">
                   <button class="btn btn-sm" [ngClass]="s.isActive ? 'btn-warning' : 'btn-success'" (click)="toggleSchedule(s)">
                     {{ s.isActive ? 'Pause' : 'Resume' }}
                   </button>
                   <button class="btn btn-danger btn-sm" (click)="deleteSchedule(s.scheduleId)">Delete</button>
                </td>
              </tr>
              <tr *ngIf="schedules.length === 0"><td colspan="6" style="text-align: center; padding: 32px;" class="text-muted">No recurring schedules set yet.</td></tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Fleet List Tab -->
      <div *ngIf="tab === 'fleet'" class="animate-fadeIn">
        <h2 class="heading-md" style="margin-bottom: 16px;">My Fleet</h2>
        <div class="table-container">
          <table>
            <thead><tr>
              <th>Bus</th><th>Route</th><th>Price</th><th>Departure</th><th>Bookings</th><th>Status</th><th>Actions</th>
            </tr></thead>
            <tbody>
              <tr *ngFor="let b of buses">
                <td><strong>{{ b.busNumber }}</strong><br><span class="text-muted">{{ b.plateNumber }}</span></td>
                <td>{{ b.sourceCity }} → {{ b.destinationCity }}</td>
                <td>₹{{ b.basePrice }}</td>
                <td>{{ b.departureTime | date:'dd MMM, hh:mm a' }}</td>
                <td>
                  <span class="badge badge-info" [class.badge-success]="(b.totalSeats - b.availableSeats) > 0">
                    👥 {{ b.totalSeats - b.availableSeats }} / {{ b.totalSeats }}
                  </span>
                </td>
                <td><span class="badge" [ngClass]="{'badge-success': b.status === 'Active', 'badge-warning': b.status === 'PendingApproval', 'badge-error': b.status === 'Unavailable' || b.status === 'Disabled' || b.status === 'Rejected' || b.status === 'EmergencyOff'}">{{ b.status }}</span></td>
                <td>
                  <div class="flex" style="gap: 8px;">
                    <button *ngIf="b.status === 'Active'" class="btn btn-danger btn-sm" (click)="toggleStatus(b, 5)">Emergency Off</button>
                    <button *ngIf="b.status === 'EmergencyOff' || b.status === 'Unavailable'" class="btn btn-success btn-sm" (click)="toggleStatus(b, 1)">Reactivate</button>
                    <button class="btn btn-secondary btn-sm" (click)="editBus(b)">Edit</button>
                    <button class="btn btn-secondary btn-sm" (click)="viewManifest(b)">Manifest</button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Revenue Detail Tab -->
      <div *ngIf="tab === 'revenue'" class="animate-fadeIn">
         <div class="card" style="margin-bottom: 32px; border-left: 4px solid var(--accent-primary);">
          <h3 class="heading-md" style="margin-bottom: 16px;">📈 Revenue Analytics</h3>
          <div class="table-container">
            <table>
              <thead><tr><th>Bus</th><th>Route</th><th>Bookings</th><th>Revenue</th></tr></thead>
              <tbody>
                <tr *ngFor="let b of revenue?.perBus">
                  <td>{{ b.busNumber }}</td><td>{{ b.route }}</td>
                  <td>{{ b.passengerCount }}</td>
                  <td class="text-accent">₹{{ b.revenue | number }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Manifest Modal -->
      <div *ngIf="manifest" class="card animate-slideDown" style="margin-top: 24px; border-top: 4px solid var(--accent-primary);">
        <div class="flex-between" style="margin-bottom: 16px;">
          <h3 class="heading-md">📋 Passenger Manifest — {{ manifest.busNumber }}</h3>
          <button class="btn btn-secondary btn-sm" (click)="manifest = null">Close</button>
        </div>
        <p class="text-muted" style="margin-bottom: 12px;">{{ manifest.route }} | {{ manifest.departure | date:'dd MMM yyyy, hh:mm a' }}</p>
        <div class="table-container">
          <table>
            <thead><tr><th>Seat</th><th>Name</th><th>Age</th><th>Gender</th><th>Mobile</th></tr></thead>
            <tbody>
              <tr *ngFor="let p of manifest.passengers">
                <td>{{ p.seatNumber }}</td><td>{{ p.name }}</td><td>{{ p.age }}</td><td>{{ p.gender }}</td><td>{{ p.mobile }}</td>
              </tr>
              <tr *ngIf="manifest.passengers.length === 0"><td colspan="5" style="text-align: center;">No passengers yet</td></tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .stat-card { text-align: center; padding: 20px; transition: var(--transition); border: 1px solid transparent; }
    .stat-label { display: block; font-size: 11px; color: var(--text-muted); margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
    .stat-value { font-size: 24px; font-weight: 800; }
    .active-tab-card { border-color: var(--accent-primary); background: var(--bg-surface-light); }
    .tabs { display: flex; gap: 4px; border-bottom: 1px solid var(--border-color); padding-bottom: 0; }
    .tab {
      padding: 12px 20px; background: none; border: none; color: var(--text-muted); font-weight: 600;
      font-size: 13px; cursor: pointer; border-bottom: 2px solid transparent;
      transition: var(--transition); font-family: 'Inter', sans-serif;
      &:hover { color: var(--text-primary); }
      &.active { color: var(--accent-primary); border-bottom-color: var(--accent-primary); }
    }
    .clickable { cursor: pointer; &:hover { transform: translateY(-4px); box-shadow: 0 8px 24px rgba(0,0,0,0.1); } }
  `]
})
export class OperatorDashboardComponent implements OnInit, OnDestroy {
  profile: any = null;
  buses: BusDto[] = [];
  schedules: BusScheduleDto[] = [];
  routes: RouteDto[] = [];
  revenue: any = null;
  manifest: any = null;
  tab = 'fleet';
  editingBusId: string | null = null;
  
  newBus = { routeId: '', plateNumber: '', busNumber: '', basePrice: 500, pickupAddress: '', dropoffAddress: '', departureTime: '', totalSeats: 42 };
  newSchedule = { routeId: '', plateNumber: '', busNumber: '', basePrice: 500, pickupAddress: 'Main Terminal', dropoffAddress: 'City Center', departureTime: '09:00', totalSeats: 42 };
  
  processing = false;
  errorMsg = '';

  private refreshTimer: any;

  constructor(private api: ApiService) {}

  ngOnInit() { 
    this.load(); 
    this.refreshTimer = setInterval(() => this.load(), 10000); 
  }

  ngOnDestroy() {
    if (this.refreshTimer) clearInterval(this.refreshTimer);
  }

  load() {
    this.api.getOperatorProfile().subscribe(r => this.profile = r.data);
    this.api.getMyBuses().subscribe(r => this.buses = r.data || []);
    this.api.getMySchedules().subscribe(r => this.schedules = r.data || []);
    this.api.getRoutes().subscribe(r => this.routes = r.data || []);
    this.api.getOperatorRevenue().subscribe(r => this.revenue = r.data);
  }

  addBus() {
    if (!this.newBus.routeId || !this.newBus.plateNumber || !this.newBus.busNumber) { this.errorMsg = 'Fill all required fields'; return; }
    this.processing = true; this.errorMsg = '';
    const payload = { ...this.newBus, totalSeats: Number(this.newBus.totalSeats) };
    this.api.createBus(payload).subscribe({
      next: r => { this.processing = false; if (r.success) { this.load(); this.tab = 'fleet'; this.resetNewBus(); } else this.errorMsg = r.message; },
      error: () => { this.processing = false; this.errorMsg = 'Failed to add bus'; }
    });
  }

  editBus(bus: BusDto) {
    this.editingBusId = bus.busId;
    this.tab = 'add';
    this.newBus = {
      routeId: bus.routeId,
      plateNumber: bus.plateNumber,
      busNumber: bus.busNumber,
      basePrice: bus.basePrice,
      pickupAddress: bus.pickupAddress,
      dropoffAddress: bus.dropoffAddress,
      departureTime: bus.departureTime.split('.')[0],
      totalSeats: bus.totalSeats
    };
    window.scrollTo({ top: 100, behavior: 'smooth' });
  }

  saveBus() {
    if (!this.editingBusId) return;
    this.processing = true;
    const payload = { ...this.newBus, totalSeats: Number(this.newBus.totalSeats) };
    this.api.updateBus(this.editingBusId, payload).subscribe({
      next: r => { 
        this.processing = false; 
        if (r.success) { this.cancelEdit(); this.load(); this.tab = 'fleet'; } else this.errorMsg = r.message; 
      },
      error: () => { this.processing = false; this.errorMsg = 'Failed to update bus'; }
    });
  }

  cancelEdit() {
    this.editingBusId = null;
    this.resetNewBus();
  }

  resetNewBus() {
    this.newBus = { routeId: '', plateNumber: '', busNumber: '', basePrice: 500, pickupAddress: '', dropoffAddress: '', departureTime: '', totalSeats: 42 };
  }

  // --- Schedule Methods ---

  addSchedule() {
    if (!this.newSchedule.routeId || !this.newSchedule.departureTime || !this.newSchedule.busNumber) { this.errorMsg = 'Fill all fields'; return; }
    this.processing = true; this.errorMsg = '';
    this.api.createSchedule(this.newSchedule).subscribe({
      next: r => {
        this.processing = false;
        if (r.success) { this.load(); this.newSchedule = { ...this.newSchedule, busNumber: '', plateNumber: '' }; }
        else this.errorMsg = r.message;
      },
      error: () => { this.processing = false; this.errorMsg = 'Failed to create schedule'; }
    });
  }

  toggleSchedule(s: BusScheduleDto) {
    this.api.toggleSchedule(s.scheduleId, !s.isActive).subscribe(r => { if (r.success) this.load(); });
  }

  deleteSchedule(id: string) {
    if (!confirm('Delete this recurring schedule? Existing bus instances will remain.')) return;
    this.api.deleteSchedule(id).subscribe(r => { if (r.success) this.load(); });
  }

  toggleStatus(bus: BusDto, status: number) {
    this.api.toggleBusStatus(bus.busId, status).subscribe(r => { if (r.success) this.load(); });
  }

  viewManifest(bus: BusDto) {
    this.api.getManifest(bus.busId).subscribe(r => { if (r.success) this.manifest = r.data; });
  }
}

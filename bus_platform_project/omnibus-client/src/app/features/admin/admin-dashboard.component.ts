import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AdminDashboardDto, OperatorProfileDto, RouteDto, BusDto } from '../../models/api.models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="page-content container">
      <h1 class="heading-xl animate-fadeIn" style="margin-bottom: 32px;">⚙️ Admin Dashboard</h1>

      <!-- Stats -->
      <div class="grid grid-4" style="margin-bottom: 40px;" *ngIf="dashboard">
        <div class="card stat-card animate-fadeIn clickable" (click)="tab='revenue'">
          <span class="stat-label">Revenue</span>
          <span class="stat-value text-accent">₹{{ dashboard.totalRevenue | number }}</span>
        </div>
        <div class="card stat-card animate-fadeIn clickable" (click)="tab='revenue'" style="animation-delay: 0.1s;">
          <span class="stat-label">Bookings</span>
          <span class="stat-value">{{ dashboard.totalBookings }}</span>
        </div>
        <div class="card stat-card animate-fadeIn clickable" (click)="tab='operators'" style="animation-delay: 0.2s;">
          <span class="stat-label">Operators</span>
          <span class="stat-value">{{ dashboard.activeOperators }} <small class="text-warning" style="font-size: 14px;">+{{ dashboard.pendingOperators }}</small></span>
        </div>
        <div class="card stat-card animate-fadeIn clickable" (click)="tab='buses'" style="animation-delay: 0.3s;">
          <span class="stat-label">Active Buses</span>
          <span class="stat-value">{{ dashboard.activeBuses }}</span>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs" style="margin-bottom: 24px;">
        <button class="tab" [class.active]="tab==='routes'" (click)="tab='routes'" id="tab-routes">🗺️ Routes</button>
        <button class="tab" [class.active]="tab==='operators'" (click)="tab='operators'" id="tab-operators">👤 Operators</button>
        <button class="tab" [class.active]="tab==='buses'" (click)="tab='buses'" id="tab-buses">🚌 Buses</button>
        <button class="tab" [class.active]="tab==='revenue'" (click)="tab='revenue'" id="tab-revenue">📈 Revenue</button>
      </div>

      <!-- Routes Tab -->
      <div *ngIf="tab === 'routes'">
        <div class="card" style="margin-bottom: 24px;">
          <h3 class="heading-sm" style="margin-bottom: 12px;">Add Route</h3>
          <div class="flex gap-md">
            <input class="form-input" [(ngModel)]="newRoute.source" placeholder="Source City" id="route-source" style="flex:1;">
            <span class="flex-center" style="font-size: 20px;">→</span>
            <input class="form-input" [(ngModel)]="newRoute.dest" placeholder="Destination City" id="route-dest" style="flex:1;">
            <button class="btn btn-primary" (click)="addRoute()" id="add-route-btn">Add</button>
          </div>
        </div>
        <div class="table-container">
          <table>
            <thead><tr><th>Route Stack</th><th>Sectors</th><th>Total Buses</th><th>Actions</th></tr></thead>
            <tbody>
              <ng-container *ngFor="let group of groupedRoutes">
                <tr class="clickable" (click)="expandedRoute = expandedRoute === group.id ? null : group.id">
                  <td><strong>{{ group.cities[0] }} ↔ {{ group.cities[1] }}</strong></td>
                  <td>{{ group.routes.length }} sectors</td>
                  <td><span class="badge badge-info">{{ group.totalBuses }} Buses</span></td>
                  <td><small class="text-muted">{{ expandedRoute === group.id ? 'Click to collapse' : 'Click to expand' }}</small></td>
                </tr>
                <tr *ngIf="expandedRoute === group.id" class="bg-surface-light">
                  <td colspan="4" style="padding: 0;">
                    <div style="padding: 16px; border-left: 4px solid var(--accent-primary);">
                      <div *ngFor="let r of group.routes" class="flex-between" style="padding: 8px 0; border-bottom: 1px solid rgba(0,0,0,0.05);">
                        <div class="flex gap-md align-center">
                          <span>{{ r.sourceCity }} → {{ r.destinationCity }}</span>
                          <span class="text-muted" style="font-size: 12px;">({{ r.busCount }} buses)</span>
                        </div>
                        <div class="flex gap-sm">
                          <div class="flex gap-xs" style="margin-right: 16px;">
                             <span *ngFor="let opName of getOperatorsForRoute(r.routeId)" 
                                   class="badge badge-outline clickable-text" 
                                   (click)="jumpToOperator(opName); $event.stopPropagation()">
                               👤 {{ opName }}
                             </span>
                          </div>
                          <button class="btn btn-danger btn-sm" (click)="deleteRoute(r.routeId); $event.stopPropagation()">Delete Sector</button>
                        </div>
                      </div>
                    </div>
                  </td>
                </tr>
              </ng-container>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Operators Tab -->
      <div *ngIf="tab === 'operators'">
        <div class="table-container">
          <table>
            <thead><tr><th>Email</th><th>Business</th><th>Status</th><th>Actions</th></tr></thead>
            <tbody>
              <ng-container *ngFor="let op of operators">
                <tr [id]="'op-row-' + op.businessName" class="clickable" (click)="expandedOp = expandedOp === op.userId ? null : op.userId">
                  <td>{{ op.email }}</td><td>{{ op.businessName }}</td>
                  <td><span class="badge" [ngClass]="{'badge-success': op.approvalStatus === 'Approved', 'badge-warning': op.approvalStatus === 'Pending', 'badge-error': op.approvalStatus === 'Rejected' || op.approvalStatus === 'Disabled'}">{{ op.approvalStatus }}</span></td>
                  <td class="flex gap-sm">
                    <button *ngIf="op.approvalStatus === 'Pending'" class="btn btn-success btn-sm" (click)="reviewOp(op.userId, true); $event.stopPropagation()">Approve</button>
                    <button *ngIf="op.approvalStatus === 'Pending'" class="btn btn-danger btn-sm" (click)="reviewOp(op.userId, false); $event.stopPropagation()">Reject</button>
                    <button *ngIf="op.approvalStatus === 'Approved'" class="btn btn-danger btn-sm" (click)="toggleOp(op.userId, false); $event.stopPropagation()">Disable</button>
                    <button *ngIf="op.approvalStatus === 'Disabled'" class="btn btn-success btn-sm" (click)="toggleOp(op.userId, true); $event.stopPropagation()">Enable</button>
                  </td>
                </tr>
                <!-- Expanded Operator Details -->
                <tr *ngIf="expandedOp === op.userId" class="bg-surface-light">
                  <td colspan="4" style="padding: 0;">
                    <div style="padding: 20px; border-left: 4px solid var(--accent-primary);">
                      <h4 class="heading-sm" style="margin-bottom: 12px;">Active Fleet</h4>
                      <div class="grid grid-3" style="gap: 12px;" *ngIf="getOperatorBuses(op.userId).length > 0; else noBuses">
                        <div *ngFor="let b of getOperatorBuses(op.userId)" class="card-flat" style="padding: 12px;">
                          <strong>{{ b.busNumber }}</strong><br>
                          <small>{{ b.sourceCity }} → {{ b.destinationCity }}</small><br>
                          <div class="flex-between align-center" style="margin-top: 8px;">
                            <span class="badge badge-sm badge-info">{{ b.status }}</span>
                            <span class="badge badge-pill badge-purple" title="Occupied Seats">
                              👥 {{ b.totalSeats - b.availableSeats }} / {{ b.totalSeats }}
                            </span>
                          </div>
                        </div>
                      </div>
                      <ng-template #noBuses><p class="text-muted">No buses registered by this operator.</p></ng-template>
                    </div>
                  </td>
                </tr>
              </ng-container>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Buses Tab -->
      <div *ngIf="tab === 'buses'">
        <div class="table-container">
          <table>
            <thead><tr><th>Bus</th><th>Route</th><th>Operator</th><th>Status</th><th>Occupancy</th><th>Actions</th></tr></thead>
            <tbody>
              <tr *ngFor="let b of allBuses">
                <td><strong>{{ b.busNumber }}</strong><br><small class="text-muted">{{ b.plateNumber }}</small></td>
                <td>{{ b.sourceCity }} → {{ b.destinationCity }}</td>
                <td>{{ b.operatorName }}</td>
                <td><span class="badge" [ngClass]="{'badge-success': b.status === 'Active', 'badge-warning': b.status === 'PendingApproval', 'badge-error': b.status === 'Disabled' || b.status === 'Rejected'}">{{ b.status }}</span></td>
                <td>
                  <div style="width: 100px;">
                    <div class="flex-between" style="font-size: 11px; margin-bottom: 4px;">
                      <span>{{ b.totalSeats - b.availableSeats }} / {{ b.totalSeats }}</span>
                      <span>{{ ((b.totalSeats - b.availableSeats)/b.totalSeats * 100) | number:'1.0-0' }}%</span>
                    </div>
                    <div style="height: 4px; background: rgba(255,255,255,0.1); border-radius: 2px;">
                      <div [style.width]="((b.totalSeats - b.availableSeats)/b.totalSeats * 100) + '%'" style="height: 100%; background: var(--accent-primary); border-radius: 2px;"></div>
                    </div>
                  </div>
                </td>
                <td>
                  <button *ngIf="b.status === 'PendingApproval'" class="btn btn-success btn-sm" (click)="approveBus(b.busId)">Approve</button>
                  <button *ngIf="b.status === 'PendingApproval'" class="btn btn-danger btn-sm" (click)="rejectBus(b.busId)" style="margin-left: 4px;">Reject</button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Revenue Analytics Tab -->
      <div *ngIf="tab === 'revenue'" class="animate-fadeIn">
        <div class="card" style="margin-bottom: 32px; background: var(--bg-surface-light); border: 1px solid var(--accent-primary);">
          <div class="flex-between align-center">
            <div>
              <span class="text-muted" style="text-transform: uppercase; font-size: 12px; letter-spacing: 1px;">Total Platform Profit</span>
              <h2 class="heading-xl text-success" style="margin-top: 8px;">₹{{ dashboard?.platformProfit | number }}</h2>
            </div>
            <div style="text-align: right;">
              <span class="text-muted" style="font-size: 12px;">From {{ dashboard?.totalBookings }} bookings</span>
              <p style="margin-top: 4px; font-weight: 600;">Total Revenue: ₹{{ dashboard?.totalRevenue | number }}</p>
            </div>
          </div>
        </div>

        <div class="grid grid-2" style="gap: 24px;">
          <div class="card">
            <h3 class="heading-md" style="margin-bottom: 16px;">📈 Top Earning Routes</h3>
            <div class="table-container">
              <table>
                <thead><tr><th>Route</th><th>Revenue</th><th>Bookings</th></tr></thead>
                <tbody>
                  <tr *ngFor="let r of analytics?.routes">
                    <td>{{ r.routeName }}</td>
                    <td class="text-accent">₹{{ r.revenue | number }}</td>
                    <td>{{ r.bookingCount }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="card">
            <h3 class="heading-md" style="margin-bottom: 16px;">🏢 Operator Performance</h3>
            <div class="table-container">
              <table>
                <thead><tr><th>Operator</th><th>Turnover</th><th>Admin Profit</th><th>Buses</th></tr></thead>
                <tbody>
                  <tr *ngFor="let op of analytics?.operators">
                    <td>{{ op.name }}</td>
                    <td class="text-accent">₹{{ op.totalTurnover | number }}</td>
                    <td class="text-success">₹{{ op.platformEarnings | number }}</td>
                    <td>{{ op.busCount }}</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .stat-card { text-align: center; padding: 24px; }
    .stat-label { display: block; font-size: 13px; color: var(--text-muted); margin-bottom: 8px; text-transform: uppercase; letter-spacing: 0.5px; }
    .stat-value { font-size: 28px; font-weight: 800; }
    .tabs { display: flex; gap: 4px; border-bottom: 1px solid var(--border-color); padding-bottom: 0; }
    .tab {
      padding: 12px 24px; background: none; border: none; color: var(--text-muted); font-weight: 600;
      font-size: 14px; cursor: pointer; border-bottom: 2px solid transparent;
      transition: var(--transition); font-family: 'Inter', sans-serif;
      &:hover { color: var(--text-primary); }
      &.active { color: var(--accent-primary); border-bottom-color: var(--accent-primary); }
    }
    .badge-purple { background: rgba(187,134,252,0.15); color: var(--accent-primary); border: 1px solid rgba(187,134,252,0.3); font-weight: 700; padding: 4px 10px; border-radius: 20px; font-size: 12px; }
    .clickable { cursor: pointer; transition: transform 0.2s; &:hover { transform: translateY(-4px); box-shadow: 0 8px 24px rgba(0,0,0,0.1); } }
  `]
})
export class AdminDashboardComponent implements OnInit {
  dashboard: AdminDashboardDto | null = null;
  analytics: any = null;
  routes: RouteDto[] = [];
  operators: OperatorProfileDto[] = [];
  allBuses: BusDto[] = [];
  tab = 'routes';
  newRoute = { source: '', dest: '' };
  expandedOp: string | null = null;
  expandedRoute: string | null = null;

  get groupedRoutes() {
    const groups: any[] = [];
    const seen = new Set<string>();

    this.routes.forEach(r => {
      if (seen.has(r.routeId)) return;
      
      const cities = [r.sourceCity, r.destinationCity].sort();
      const groupId = cities.join('-');
      
      const existing = groups.find(g => g.id === groupId);
      if (existing) {
        existing.routes.push(r);
        existing.totalBuses += r.busCount;
      } else {
        groups.push({
          id: groupId,
          cities: cities,
          routes: [r],
          totalBuses: r.busCount
        });
      }
      seen.add(r.routeId);
    });
    return groups;
  }

  getOperatorBuses(userId: string) {
    return this.allBuses.filter(b => b.operatorId === userId);
  }

  getOperatorsForRoute(routeId: string) {
    const busOperators = this.allBuses
      .filter(b => b.routeId === routeId)
      .map(b => b.operatorName);
    return [...new Set(busOperators)];
  }

  jumpToOperator(businessName: string) {
    this.tab = 'operators';
    const op = this.operators.find(o => o.businessName === businessName);
    if (op) {
      this.expandedOp = op.userId;
      setTimeout(() => {
        const el = document.getElementById('op-row-' + businessName);
        el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }, 100);
    }
  }

  private refreshTimer: any;

  constructor(private api: ApiService) {}

  ngOnInit() { 
    this.load(); 
    this.refreshTimer = setInterval(() => this.load(), 10000); // Auto-refresh every 10s
  }

  ngOnDestroy() {
    if (this.refreshTimer) clearInterval(this.refreshTimer);
  }

  load() {
    this.api.getAdminDashboard().subscribe(r => this.dashboard = r.data);
    this.api.getAdminRevenueAnalytics().subscribe(r => this.analytics = r.data);
    this.api.getRoutes().subscribe(r => this.routes = r.data || []);
    this.api.getOperators().subscribe(r => this.operators = r.data || []);
    this.api.getAllBuses().subscribe(r => this.allBuses = r.data || []);
  }

  addRoute() {
    if (!this.newRoute.source || !this.newRoute.dest) return;
    this.api.createRoute(this.newRoute.source, this.newRoute.dest).subscribe(r => {
      if (r.success) { this.newRoute = { source: '', dest: '' }; this.load(); }
    });
  }

  deleteRoute(id: string) {
    if (!confirm('Delete this route?')) return;
    this.api.deleteRoute(id).subscribe(() => this.load());
  }

  reviewOp(userId: string, approve: boolean) {
    const reason = approve ? undefined : prompt('Rejection reason:') || undefined;
    this.api.reviewOperator(userId, approve, reason).subscribe(() => this.load());
  }

  toggleOp(userId: string, enable: boolean) {
    const action = enable ? 'enable' : 'disable';
    const msg = enable ? 'Re-activate this operator?' : 'Disable this operator? All future bookings will be refunded.';
    if (!confirm(msg)) return;
    this.api.toggleOperator(userId, enable).subscribe(r => {
      if (r.success) { alert(r.message); this.load(); }
    });
  }

  approveBus(busId: string) {
    this.api.approveBus(busId).subscribe(() => this.load());
  }

  rejectBus(busId: string) {
    if (!confirm('Reject this bus application?')) return;
    this.api.rejectBus(busId).subscribe(() => this.load());
  }
}

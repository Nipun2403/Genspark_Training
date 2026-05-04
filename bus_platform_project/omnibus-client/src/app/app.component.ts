import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, CommonModule],
  template: `
    <nav class="navbar">
      <div class="container flex-between">
        <a routerLink="/" class="logo">
          <span class="logo-icon">🚌</span>
          <span class="logo-text">Omni<span class="text-accent">Bus</span></span>
        </a>
        <div class="nav-links">
          <a *ngIf="!auth.isLoggedIn || auth.role === 'Customer'" routerLink="/search" class="nav-link">All Buses</a>
          <ng-container *ngIf="auth.isLoggedIn">
            <a *ngIf="auth.role === 'Customer'" routerLink="/my-bookings" class="nav-link">My Bookings</a>
          </ng-container>
        </div>
        <div class="nav-right">
          <ng-container *ngIf="!auth.isLoggedIn">
            <a routerLink="/login" class="btn btn-primary btn-sm">Login</a>
          </ng-container>
          <ng-container *ngIf="auth.isLoggedIn">
            <div class="user-menu">
              <span class="user-avatar">{{ (auth.currentUser?.fullName || auth.currentUser?.email || '?')[0] | uppercase }}</span>
              <div class="user-dropdown">
                <div class="user-info">
                  <span class="user-name">{{ auth.currentUser?.fullName || 'User' }}</span>
                  <span class="user-email">{{ auth.currentUser?.email }}</span>
                  <span class="badge badge-info">{{ auth.currentUser?.role }}</span>
                </div>
                <hr style="border-color: var(--border-color); margin: 8px 0;">
                <a routerLink="/my-bookings" class="dropdown-item">My Bookings</a>
                <a *ngIf="auth.role === 'Customer'" routerLink="/operator/register" class="dropdown-item">Become an Operator</a>
                <button class="dropdown-item text-error" (click)="logout()">Logout</button>
              </div>
            </div>
          </ng-container>
        </div>
      </div>
    </nav>
    <main>
      <router-outlet></router-outlet>
    </main>
  `,
  styles: [`
    .navbar {
      background: rgba(18,18,18,0.95); backdrop-filter: blur(16px);
      border-bottom: 1px solid var(--border-color); padding: 0 0;
      position: sticky; top: 0; z-index: 100; height: 72px;
      .container { height: 100%; }
    }
    .logo { display: flex; align-items: center; gap: 10px; text-decoration: none; }
    .logo-icon { font-size: 28px; }
    .logo-text { font-size: 22px; font-weight: 800; color: var(--text-primary); letter-spacing: -0.5px; }
    .nav-links { display: flex; gap: 8px; }
    .nav-link {
      padding: 8px 16px; color: var(--text-secondary); font-weight: 500; font-size: 14px;
      border-radius: var(--radius-sm); transition: var(--transition);
      &:hover { color: var(--text-primary); background: var(--bg-surface); }
    }
    .nav-right { display: flex; align-items: center; gap: 12px; }
    .user-menu { position: relative; cursor: pointer; }
    .user-avatar {
      width: 40px; height: 40px; border-radius: 50%; background: var(--accent-primary);
      color: #121212; display: flex; align-items: center; justify-content: center;
      font-weight: 700; font-size: 16px; transition: var(--transition);
      &:hover { transform: scale(1.05); box-shadow: 0 0 0 3px rgba(187,134,252,0.3); }
    }
    .user-dropdown {
      position: absolute; top: 52px; right: 0; width: 240px;
      background: var(--bg-surface); border: 1px solid var(--border-color);
      border-radius: var(--radius-md); padding: 12px; box-shadow: var(--shadow-lg);
      opacity: 0; visibility: hidden; transform: translateY(10px);
      transition: all 0.2s ease, visibility 0s linear 0.15s;
    }
    .user-menu:hover .user-dropdown { opacity: 1; visibility: visible; transform: translateY(0); transition-delay: 0s; }
    .user-info { display: flex; flex-direction: column; gap: 4px; padding: 4px 0; }
    .user-name { font-weight: 600; font-size: 14px; }
    .user-email { font-size: 12px; color: var(--text-muted); }
    .dropdown-item {
      display: block; width: 100%; padding: 8px 12px; border-radius: var(--radius-sm);
      font-size: 14px; color: var(--text-secondary); background: none; border: none;
      text-align: left; cursor: pointer; font-family: 'Inter', sans-serif;
      transition: var(--transition);
      &:hover { background: var(--bg-surface-light); color: var(--text-primary); }
    }
    @media (max-width: 768px) {
      .nav-links { display: none; }
    }
  `]
})
export class AppComponent {
  constructor(public auth: AuthService) {}
  logout() { this.auth.logout(); window.location.href = '/'; }
}

import { Routes } from '@angular/router';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  { path: 'search', loadComponent: () => import('./features/search/search-results.component').then(m => m.SearchResultsComponent) },
  { path: 'bus/:busId/seats', loadComponent: () => import('./features/booking/seat-map.component').then(m => m.SeatMapComponent), canActivate: [authGuard] },
  { path: 'booking/:bookingId/passengers', loadComponent: () => import('./features/booking/passenger-form.component').then(m => m.PassengerFormComponent), canActivate: [authGuard] },
  { path: 'checkout/:id', loadComponent: () => import('./features/booking/checkout.component').then(m => m.CheckoutComponent), canActivate: [authGuard] },
  { path: 'payment/:bookingId', loadComponent: () => import('./features/booking/payment-sim.component').then(m => m.PaymentSimComponent), canActivate: [authGuard] },
  { path: 'my-bookings', loadComponent: () => import('./features/customer/my-bookings.component').then(m => m.MyBookingsComponent), canActivate: [authGuard] },
  { path: 'operator/register', loadComponent: () => import('./features/operator/operator-register.component').then(m => m.OperatorRegisterComponent), canActivate: [authGuard] },
  { path: 'operator/dashboard', loadComponent: () => import('./features/operator/operator-dashboard.component').then(m => m.OperatorDashboardComponent), canActivate: [roleGuard(['Operator'])] },
  { path: 'admin', loadComponent: () => import('./features/admin/admin-dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [roleGuard(['Admin'])] },
  { path: '**', redirectTo: '' }
];

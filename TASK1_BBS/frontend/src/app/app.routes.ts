import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/user/home', pathMatch: 'full' },

  // Auth
  { path: 'auth/login',             loadComponent: () => import('./auth/login/login.component').then(m => m.LoginComponent) },
  { path: 'auth/register',          loadComponent: () => import('./auth/register/register.component').then(m => m.RegisterComponent) },
  { path: 'auth/operator-register', loadComponent: () => import('./auth/operator-register/operator-register.component').then(m => m.OperatorRegisterComponent) },

  // User / Customer (search is public, rest protected)
  { path: 'user/home',       loadComponent: () => import('./user/home/home.component').then(m => m.HomeComponent) },
  { path: 'user/bus-layout', loadComponent: () => import('./user/bus-layout/bus-layout.component').then(m => m.BusLayoutComponent), canActivate: [authGuard] },
  { path: 'user/bookings',   loadComponent: () => import('./user/bookings/bookings.component').then(m => m.BookingsComponent),       canActivate: [authGuard, roleGuard], data: { role: 'Customer' } },
  { path: 'user/profile',    loadComponent: () => import('./user/profile/profile.component').then(m => m.ProfileComponent),           canActivate: [authGuard, roleGuard], data: { role: 'Customer' } },

  // Operator
  { path: 'operator/dashboard', loadComponent: () => import('./operator/dashboard/dashboard.component').then(m => m.OperatorDashboardComponent), canActivate: [authGuard, roleGuard], data: { role: 'Operator' } },
  { path: 'operator/buses',     loadComponent: () => import('./operator/buses/buses.component').then(m => m.OperatorBusesComponent),             canActivate: [authGuard, roleGuard], data: { role: 'Operator' } },
  { path: 'operator/schedules', loadComponent: () => import('./operator/schedules/schedules.component').then(m => m.OperatorSchedulesComponent), canActivate: [authGuard, roleGuard], data: { role: 'Operator' } },
  { path: 'operator/layouts',   loadComponent: () => import('./operator/layouts/layouts.component').then(m => m.OperatorLayoutsComponent),       canActivate: [authGuard, roleGuard], data: { role: 'Operator' } },

  // Admin
  { path: 'admin/dashboard', loadComponent: () => import('./admin/dashboard/dashboard.component').then(m => m.AdminDashboardComponent), canActivate: [authGuard, roleGuard], data: { role: 'Admin' } },
  { path: 'admin/operators', loadComponent: () => import('./admin/operators/operators.component').then(m => m.AdminOperatorsComponent), canActivate: [authGuard, roleGuard], data: { role: 'Admin' } },
  { path: 'admin/buses',     loadComponent: () => import('./admin/buses/buses.component').then(m => m.AdminBusesComponent),             canActivate: [authGuard, roleGuard], data: { role: 'Admin' } },
  { path: 'admin/routes',    loadComponent: () => import('./admin/routes/routes.component').then(m => m.AdminRoutesComponent),           canActivate: [authGuard, roleGuard], data: { role: 'Admin' } },

  { path: '**', redirectTo: '/user/home' }
];

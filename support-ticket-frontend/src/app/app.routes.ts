import { Routes } from '@angular/router';

import { authGuard } from './guards/auth.guard';


export const routes: Routes = [
     {
    path: 'login',
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent),
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  {
    path: 'tickets',
    loadComponent: () => import('./pages/ticket-list/ticket-list.component').then(m => m.TicketListComponent),
    canActivate: [authGuard],
  },
  {
    path: 'tickets/new',
    loadComponent: () => import('./pages/ticket-create/ticket-create.component').then(m => m.TicketCreateComponent),
    canActivate: [authGuard],
  },
  {
    path: 'tickets/:id',
    loadComponent: () => import('./pages/ticket-detail/ticket-detail.component').then(m => m.TicketDetailComponent),
    canActivate: [authGuard],
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent),
    canActivate: [authGuard],
  },
  {
    path: 'users',
    loadComponent: () => import('./pages/user-management/user-management.component').then(m => m.UserManagementComponent),
    canActivate: [authGuard],
  }
];

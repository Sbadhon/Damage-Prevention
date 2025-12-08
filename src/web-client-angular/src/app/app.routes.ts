import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: 'tickets',
    loadComponent: () =>
      import('./features/tickets/tickets-dashboard/tickets-dashboard').then(
        (m) => m.TicketsDashboard
      ),
  },
  {
    path: 'workorders',
    loadComponent: () =>
      import('./features/workorders/workorders-dashboard/workorders-dashboard').then(
        (m) => m.WorkordersDashboard
      ),
  },
  {
    path: 'risk',
    loadComponent: () =>
      import('./features/risk/risk-dashboard/risk-dashboard').then(
        (m) => m.RiskDashboard
      ),
  },
  { path: '', pathMatch: 'full', redirectTo: 'tickets' },
  { path: '**', redirectTo: 'tickets' },
];

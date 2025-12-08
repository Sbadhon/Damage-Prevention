import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZonelessChangeDetection,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { routes } from './app.routes';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { TicketEffects } from './core/state/ticket/ticket.effects';
import { WorkOrderEffects } from './core/state/workorders/workorders.effects';
import { RiskEffects } from './core/state/risk/risk.effects';
import { ticketReducer } from './core/state/ticket/ticket.reducer';
import { workOrderReducer } from './core/state/workorders/workorders.reducer';
import { riskReducer } from './core/state/risk/risk.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    provideStore({
      tickets: ticketReducer,
      workorders: workOrderReducer,
      risk: riskReducer,
    }),
    provideEffects([TicketEffects, WorkOrderEffects, RiskEffects]),
    provideStoreDevtools({ maxAge: 25, logOnly: false }),
  ],
};

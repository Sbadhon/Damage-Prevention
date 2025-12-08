import { createFeatureSelector, createSelector } from '@ngrx/store';
import { TICKETS_FEATURE_KEY, TicketState } from './ticket.reducer';

export const selectTicketsState =
  createFeatureSelector<TicketState>(TICKETS_FEATURE_KEY);

  export const selectTickets = createSelector(
    selectTicketsState,
    (state) => state.tickets?.items ?? []
  );

  export const selectTicketsTotalCount = createSelector(
    selectTicketsState,
    (state) => state.tickets?.totalCount ?? 0
  );
  
  export const selectTicketsLoading = createSelector(
    selectTicketsState,
    (state) => state.loading
  );
  
  export const selectTicketsError = createSelector(
    selectTicketsState,
    (state) => state.error
  );
  


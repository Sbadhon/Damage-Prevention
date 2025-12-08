import { createReducer, on } from '@ngrx/store';
import * as TicketActions from './ticket.actions';
import { Ticket } from './ticket.models';
import { PagedResponse } from '../util/util.model';

export const TICKETS_FEATURE_KEY = 'tickets';

export interface TicketState {
  tickets: PagedResponse<Ticket> | null;
  loading: boolean;
  error: unknown | null;
}

export const initialState: TicketState = {
  tickets: null,
  loading: false,
  error: null,
};

export const ticketReducer = createReducer(
  initialState,

  // Load Tickets
  on(TicketActions.loadTickets, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(TicketActions.loadTicketsSuccess, (state, { tickets }) => ({
    ...state,
    tickets,
    loading: false,
  })),
  on(TicketActions.loadTicketsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Create Ticket
  on(TicketActions.createTicket, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(TicketActions.createTicketSuccess, (state, { ticket }) => ({
    ...state,
    tickets: state.tickets
      ? {
          ...state.tickets,
          items: [ticket, ...state.tickets.items],
        }
      : {
          items: [ticket],
          totalCount: 1,
          pageNumber: 1,
          pageSize: 10,
        },
    loading: false,
  })),

  on(TicketActions.createTicketFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Update Ticket Status
  on(TicketActions.updateTicketStatus, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(TicketActions.updateTicketStatusSuccess, (state, { ticketId, newStatus }) => ({
    ...state,
    tickets: state.tickets
      ? {
          ...state.tickets,
          items: state.tickets.items.map((t) =>
            t.ticketId === ticketId ? { ...t, status: newStatus } : t,
          ),
        }
      : state.tickets,
    loading: false,
  })),
  on(TicketActions.updateTicketStatusFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),
);

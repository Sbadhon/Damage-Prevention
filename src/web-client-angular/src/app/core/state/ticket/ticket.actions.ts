import { createAction, props } from '@ngrx/store';
import { Ticket } from './ticket.models';
import { ListParams, PagedResponse } from '../util/util.model';

// Load Tickets
export const loadTickets = createAction(
  '[Tickets] Load',
  props<{ params: ListParams }>()
);

export const loadTicketsSuccess = createAction(
  '[Tickets] Load Success',
  props<{ tickets: PagedResponse<Ticket> }>()
);

export const loadTicketsFailure = createAction(
  '[Tickets] Load Failure',
  props<{ error: unknown }>()
);

// Create Ticket
export const createTicket = createAction(
  '[Tickets] Create',
  props<{ ticket: Omit<Ticket, 'ticketId' | 'status' | 'createdAt'> }>()
);

export const createTicketSuccess = createAction(
  '[Tickets] Create Success',
  props<{ ticket: Ticket }>()
);

export const createTicketFailure = createAction(
  '[Tickets] Create Failure',
  props<{ error: unknown }>()
);

// Update Ticket Status
export const updateTicketStatus = createAction(
  '[Tickets] Update Status',
  props<{ ticketId: string; newStatus: Ticket['status']; reason?: string }>()
);

export const updateTicketStatusSuccess = createAction(
  '[Tickets] Update Status Success',
  props<{ ticketId: string; newStatus: Ticket['status'] }>()
);

export const updateTicketStatusFailure = createAction(
  '[Tickets] Update Status Failure',
  props<{ error: unknown }>()
);

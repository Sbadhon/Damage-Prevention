import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import * as TicketActions from './ticket.actions';
import { catchError, map, mergeMap, of, switchMap } from 'rxjs';
import { TicketService } from './ticket.api';

@Injectable()
export class TicketEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(TicketService);

  // Load Tickets
  load$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TicketActions.loadTickets),
      switchMap(({ params }) =>
        this.api.listTickets(params).pipe(
          map((tickets) => TicketActions.loadTicketsSuccess({ tickets })),
          catchError((error) =>
            of(TicketActions.loadTicketsFailure({ error }))
          )
        )
      )
    )
  );

  // Create Ticket
  create$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TicketActions.createTicket),
      mergeMap(({ ticket }) =>
        this.api.createTicket(ticket).pipe(
          map((ticket) => TicketActions.createTicketSuccess({ ticket })),
          catchError((error) =>
            of(TicketActions.createTicketFailure({ error }))
          )
        )
      )
    )
  );

  // Update Ticket Status
  updateStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(TicketActions.updateTicketStatus),
      mergeMap(({ ticketId, newStatus, reason }) =>
        this.api.updateTicketStatus(ticketId, newStatus, reason).pipe(
          map(() =>
            TicketActions.updateTicketStatusSuccess({ ticketId, newStatus })
          ),
          catchError((error) =>
            of(TicketActions.updateTicketStatusFailure({ error }))
          )
        )
      )
    )
  );
}

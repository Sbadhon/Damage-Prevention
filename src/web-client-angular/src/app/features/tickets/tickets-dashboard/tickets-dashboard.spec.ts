import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { TicketsDashboard } from './tickets-dashboard';
import * as TicketActions from '@app/core/state/ticket/ticket.actions';
import * as TicketSelectors from '@app/core/state/ticket/ticket.selectors';
import { TicketStatus } from '@app/core/state/ticket/ticket.models';

describe('TicketsDashboard', () => {
  let fixture: ComponentFixture<TicketsDashboard>;
  let component: TicketsDashboard;
  let store: MockStore;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  const ticketsState = {
    tickets: {
      items: [],
      total: 0,
    },
    loading: false,
  };

  beforeEach(async () => {
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [TicketsDashboard],
      providers: [
        provideZonelessChangeDetection(),
        provideMockStore({
          selectors: [
            {
              selector: TicketSelectors.selectTicketsState,
              value: ticketsState,
            },
          ],
        }),
      ],
    })
      .overrideProvider(MatDialog, { useValue: dialogSpy })
      .compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(TicketsDashboard);
    component = fixture.componentInstance;
  });

  it('openAddTicket should open TicketForm dialog and dispatch createTicket when result returned', () => {
    const payload: any = { ticketId: 'T-1', status: TicketStatus.Open };

    const afterClosed$ = of(payload);
    dialogSpy.open.and.returnValue({ afterClosed: () => afterClosed$ } as any);

    const dispatchSpy = spyOn(store, 'dispatch');

    component.openAddTicket();

    expect(dialogSpy.open).toHaveBeenCalled();
    expect(dispatchSpy).toHaveBeenCalledWith(TicketActions.createTicket({ ticket: payload }));
  });

  it('openTicketDetails should open dialog and dispatch updateTicketStatus when status changed', () => {
    const ticket: any = { ticketId: 'T-1', status: TicketStatus.Open };

    const afterClosed$ = of({
      statusChanged: true,
      newStatus: TicketStatus.Completed,
    });
    dialogSpy.open.and.returnValue({ afterClosed: () => afterClosed$ } as any);

    const dispatchSpy = spyOn(store, 'dispatch');

    component.openTicketDetails(ticket);

    expect(dialogSpy.open).toHaveBeenCalled();
    expect(dispatchSpy).toHaveBeenCalledWith(
      TicketActions.updateTicketStatus({
        ticketId: 'T-1',
        newStatus: TicketStatus.Completed,
      }),
    );
  });
});

import { TestBed, ComponentFixture } from '@angular/core/testing';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { provideZonelessChangeDetection } from '@angular/core';
import { TicketDetail, TicketDetailData } from './ticket-detail';
import { Ticket, TicketStatus } from '@app/core/state/ticket/ticket.models';
import { updateTicketStatus } from '@app/core/state/ticket/ticket.actions';
import * as RiskActions from '@app/core/state/risk/risk.actions';
import * as RiskSelectors from '@app/core/state/risk/risk.selectors';

describe('TicketDetail', () => {
  let fixture: ComponentFixture<TicketDetail>;
  let component: TicketDetail;

  let storeSpy: jasmine.SpyObj<Store>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<TicketDetail>>;

  const mockTicket: Ticket = {
    ticketId: 'T-123',
    description: 'Locate fiber near main road',
    status: TicketStatus.Open,
    crewId: 'crew-1',
    workType: 'Locate',
    address: '123 Main St',
    createdAt: '2025-01-01T00:00:00.000Z',
    lat: 44.9,
    lon: -93.2,
  };

  const dialogData: TicketDetailData = {
    ticket: mockTicket,
  };

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj('Store', ['dispatch', 'select']);
    dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);

    // risk$ selector
    storeSpy.select.and.callFake((selector: any) => {
      if (selector === RiskSelectors.selectSelectedRisk) {
        return of(null);
      }
      return of(null);
    });

    await TestBed.configureTestingModule({
      imports: [TicketDetail],
      providers: [
        provideZonelessChangeDetection(),
        { provide: Store, useValue: storeSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(TicketDetail);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize ticket and selectedStatus from dialog data', () => {
    expect(component.ticket).toEqual(mockTicket);
    expect(component.selectedStatus).toBe(mockTicket.status);
  });

  it('should dispatch loadRiskByTicket on init', () => {
    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      RiskActions.loadRiskByTicket({ ticketId: mockTicket.ticketId }),
    );
  });

  it('should compute allowed status transitions on init', () => {
    // For Open + crew assigned, expect Open, InProgress, Completed, Cancelled
    const options = component.statusOptions;
    expect(options).toEqual([
      TicketStatus.Open,
      TicketStatus.InProgress,
      TicketStatus.Completed,
      TicketStatus.Cancelled,
    ]);
  });

  it('getAllowedTransitions should handle Open without crew', () => {
    const transitions = component.getAllowedTransitions(TicketStatus.Open, null);
    // No InProgress when no crew
    expect(transitions).toEqual([
      TicketStatus.Open,
      TicketStatus.Completed,
      TicketStatus.Cancelled,
    ]);
  });

  it('getAllowedTransitions should handle InProgress', () => {
    const transitions = component.getAllowedTransitions(
      TicketStatus.InProgress,
      'crew-1',
    );
    expect(transitions).toEqual([
      TicketStatus.InProgress,
      TicketStatus.Completed,
      TicketStatus.Cancelled,
    ]);
  });

  it('getAllowedTransitions should lock Completed and Cancelled', () => {
    const completedTransitions = component.getAllowedTransitions(
      TicketStatus.Completed,
      'crew-1',
    );
    const cancelledTransitions = component.getAllowedTransitions(
      TicketStatus.Cancelled,
      'crew-1',
    );

    expect(completedTransitions).toEqual([TicketStatus.Completed]);
    expect(cancelledTransitions).toEqual([TicketStatus.Cancelled]);
  });

  it('onStatusChange should update selectedStatus and dispatch updateTicketStatus', () => {
    const newStatus = TicketStatus.InProgress;

    component.onStatusChange(newStatus);

    expect(component.selectedStatus).toBe(newStatus);
    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      updateTicketStatus({
        ticketId: mockTicket.ticketId,
        newStatus,
      }),
    );
  });

  it('close should close the dialog', () => {
    component.close();
    expect(dialogRefSpy.close).toHaveBeenCalled();
  });
});

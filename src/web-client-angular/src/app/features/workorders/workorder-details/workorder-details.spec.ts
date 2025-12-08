import { TestBed, ComponentFixture } from '@angular/core/testing';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';
import { Store } from '@ngrx/store';
import { of } from 'rxjs';
import { provideZonelessChangeDetection } from '@angular/core';
import { WorkOrderDetail, WorkOrderDetailData } from './workorder-details';
import {
  Crew,
  WorkOrder,
  WorkOrderStatus,
} from '@app/core/state/workorders/workorders.models';
import * as WorkOrderSelectors from '@app/core/state/workorders/workorders.selectors';
import * as WorkOrderActions from '@app/core/state/workorders/workorders.actions';

describe('WorkOrderDetail', () => {
  let fixture: ComponentFixture<WorkOrderDetail>;
  let component: WorkOrderDetail;

  let storeSpy: jasmine.SpyObj<Store>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<WorkOrderDetail>>;

  const mockWorkOrder: WorkOrder = {
    workOrderId: 'WO-1',
    ticketId: 'T-1',
    crewId: 'c1',
    crewName: 'Crew Alpha',
    scheduledAt: '2025-01-01T10:00:00Z',
    status: WorkOrderStatus.Pending,
    details: 'Locate fiber near road',
    createdAt: ''
  };

  const dialogData: WorkOrderDetailData = {
    workOrder: mockWorkOrder,
  };

  beforeEach(async () => {
    storeSpy = jasmine.createSpyObj('Store', ['dispatch', 'select']);
    dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['close']);

    // Stub selectCrews
    storeSpy.select.and.callFake((selector: any) => {
      if (selector === WorkOrderSelectors.selectCrews) {
        const crews: Crew[] = [
          {
            crewId: 'c1', crewName: 'Crew Alpha',
            specialty: ''
          },
          {
            crewId: 'c2', crewName: 'Crew Beta',
            specialty: ''
          },
        ];
        return of(crews);
      }
      return of(null);
    });

    await TestBed.configureTestingModule({
      imports: [WorkOrderDetail],
      providers: [
        provideZonelessChangeDetection(),
        { provide: Store, useValue: storeSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: dialogData },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(WorkOrderDetail);
    component = fixture.componentInstance;
    fixture.detectChanges(); // triggers ngOnInit
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize workOrder, currentStatus and selectedCrew from dialog data', () => {
    expect(component.workOrder).toEqual(mockWorkOrder);
    expect(component.currentStatus).toBe(mockWorkOrder.status);
    expect(component.selectedCrew).toBe(mockWorkOrder.crewName ?? '');
  });

  it('should dispatch loadCrews on init and subscribe to selectCrews', () => {
    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      WorkOrderActions.loadCrews(),
    );

    // crews should have been populated from the selector
    expect(component.crews.length).toBe(2);
    expect(component.crews[0].crewName).toBe('Crew Alpha');
  });

  it('assignCrew should do nothing when selectedCrew is empty', () => {
    component.selectedCrew = '';
    component.crews = [
      {
        crewId: 'c1', crewName: 'Crew Alpha',
        specialty: ''
      },
      {
        crewId: 'c2', crewName: 'Crew Beta',
        specialty: ''
      },
    ];

    component.assignCrew();

    expect(storeSpy.dispatch).not.toHaveBeenCalledWith(
      WorkOrderActions.assignCrew(jasmine.anything() as any),
    );
    expect(dialogRefSpy.close).not.toHaveBeenCalledWith(
      jasmine.objectContaining({ assigned: true }),
    );
  });

  it('assignCrew should do nothing when selectedCrew equals current crewName', () => {
    component.selectedCrew = 'Crew Alpha'; // same as workOrder.crewName
    component.crews = [
      {
        crewId: 'c1', crewName: 'Crew Alpha',
        specialty: ''
      },
      {
        crewId: 'c2', crewName: 'Crew Beta',
        specialty: ''
      },
    ];

    component.assignCrew();

    expect(storeSpy.dispatch).not.toHaveBeenCalledWith(
      WorkOrderActions.assignCrew(jasmine.anything() as any),
    );
    expect(dialogRefSpy.close).not.toHaveBeenCalledWith(
      jasmine.objectContaining({ assigned: true }),
    );
  });

  it('assignCrew should dispatch assignCrew and close dialog when valid crew selected', () => {
    component.selectedCrew = 'Crew Beta';
    component.crews = [
      {
        crewId: 'c1', crewName: 'Crew Alpha',
        specialty: ''
      },
      {
        crewId: 'c2', crewName: 'Crew Beta',
        specialty: ''
      },
    ];

    component.assignCrew();

    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      WorkOrderActions.assignCrew({
        workOrderId: mockWorkOrder.workOrderId,
        crewId: 'c2',
      }),
    );

    expect(dialogRefSpy.close).toHaveBeenCalledWith({
      assigned: true,
      crewId: 'c2',
    });
  });

  it('close should close dialog with assigned: false', () => {
    component.close();
    expect(dialogRefSpy.close).toHaveBeenCalledWith({ assigned: false });
  });

  it('getAllowedTransitions should return correct transitions', () => {
    const pending = component.getAllowedTransitions(WorkOrderStatus.Pending);
    const assigned = component.getAllowedTransitions(WorkOrderStatus.Assigned);
    const completed = component.getAllowedTransitions(WorkOrderStatus.Completed);
    const cancelled = component.getAllowedTransitions(WorkOrderStatus.Cancelled);

    expect(pending).toEqual([
      WorkOrderStatus.Pending,
      WorkOrderStatus.Assigned,
      WorkOrderStatus.Cancelled,
    ]);
    expect(assigned).toEqual([
      WorkOrderStatus.Assigned,
      WorkOrderStatus.Completed,
      WorkOrderStatus.Cancelled,
    ]);
    expect(completed).toEqual([WorkOrderStatus.Completed]);
    expect(cancelled).toEqual([WorkOrderStatus.Cancelled]);
  });

  it('updateStatus should update currentStatus and dispatch updateWorkOrderStatus', () => {
    component.updateStatus(WorkOrderStatus.Completed);

    expect(component.currentStatus).toBe(WorkOrderStatus.Completed);
    expect(storeSpy.dispatch).toHaveBeenCalledWith(
      WorkOrderActions.updateWorkOrderStatus({
        id: mockWorkOrder.workOrderId,
        status: WorkOrderStatus.Completed,
      }),
    );
  });

  it('ngOnDestroy should complete destroy$ without throwing', () => {
    expect(() => component.ngOnDestroy()).not.toThrow();
  });
});

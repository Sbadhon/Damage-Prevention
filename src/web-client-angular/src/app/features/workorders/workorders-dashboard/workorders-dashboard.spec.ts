import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { MockStore, provideMockStore } from '@ngrx/store/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { WorkordersDashboard } from './workorders-dashboard';
import * as WorkOrderSelectors from '@app/core/state/workorders/workorders.selectors';
import * as WorkOrderActions from '@app/core/state/workorders/workorders.actions';
import { WorkOrderStatus } from '@app/core/state/workorders/workorders.models';

describe('WorkordersDashboard', () => {
  let fixture: ComponentFixture<WorkordersDashboard>;
  let component: WorkordersDashboard;
  let store: MockStore;
  let dialogSpy: jasmine.SpyObj<MatDialog>;

  const workOrdersState = {
    workOrders: {
      items: [],
      total: 0,
    },
    loading: false,
  };

  beforeEach(async () => {
    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    await TestBed.configureTestingModule({
      imports: [WorkordersDashboard],
      providers: [
        provideZonelessChangeDetection(),
        provideMockStore({
          selectors: [
            {
              selector: WorkOrderSelectors.selectWorkOrdersState,
              value: workOrdersState,
            },
          ],
        }),
      ],
    })
      .overrideProvider(MatDialog, { useValue: dialogSpy })
      .compileComponents();

    store = TestBed.inject(MockStore);
    fixture = TestBed.createComponent(WorkordersDashboard);
    component = fixture.componentInstance;
  });

  it('openWorkOrderDetails should open dialog and dispatch updateWorkOrderStatus when status changed', () => {
    const workOrder: any = {
      workOrderId: 'WO-1',
      status: WorkOrderStatus.Pending,
    };

    const afterClosed$ = of({
      statusChanged: true,
      newStatus: WorkOrderStatus.Completed,
    });
    dialogSpy.open.and.returnValue({ afterClosed: () => afterClosed$ } as any);

    const dispatchSpy = spyOn(store, 'dispatch');

    component.openWorkOrderDetails(workOrder);

    expect(dialogSpy.open).toHaveBeenCalled();
    expect(dispatchSpy).toHaveBeenCalledWith(
      WorkOrderActions.updateWorkOrderStatus({
        id: 'WO-1',
        status: WorkOrderStatus.Completed,
      }),
    );
  });
});

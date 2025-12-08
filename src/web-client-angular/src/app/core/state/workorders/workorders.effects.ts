import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import * as WorkOrderActions from './workorders.actions';
import { mergeMap, map, catchError, switchMap, of } from 'rxjs';
import { WorkOrderService } from './workorders.api';

@Injectable()
export class WorkOrderEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(WorkOrderService);

  loadWorkOrders$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WorkOrderActions.loadWorkOrders),
      switchMap(({ params }) =>
        this.api.listWorkOrders(params).pipe(
          map((workOrders) =>
            WorkOrderActions.loadWorkOrdersSuccess({ workOrders })
          ),
          catchError((error) =>
            of(WorkOrderActions.loadWorkOrdersFailure({ error }))
          )
        )
      )
    )
  );

  updateStatus$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WorkOrderActions.updateWorkOrderStatus),
      mergeMap(({ id, status }) =>
        this.api.updateWorkOrderStatus(id, status).pipe(
          map((workOrder) =>
            WorkOrderActions.updateWorkOrderStatusSuccess({ workOrder })
          ),
          catchError((error) =>
            of(WorkOrderActions.updateWorkOrderStatusFailure({ error }))
          )
        )
      )
    )
  );

  assignCrew$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WorkOrderActions.assignCrew),
      mergeMap(({ workOrderId, crewId }) =>
        this.api.assignCrew(workOrderId, crewId).pipe(
          map((workOrder) =>
            WorkOrderActions.assignCrewSuccess({ workOrder })
          ),
          catchError((error) =>
            of(WorkOrderActions.assignCrewFailure({ error }))
          )
        )
      )
    )
  );

  loadCrews$ = createEffect(() =>
    this.actions$.pipe(
      ofType(WorkOrderActions.loadCrews),
      mergeMap(() =>
        this.api.listCrews().pipe(
          map((crews) => WorkOrderActions.loadCrewsSuccess({ crews })),
          catchError((error) => of(WorkOrderActions.loadCrewsFailure({ error })))
        )
      )
    )
  );
}

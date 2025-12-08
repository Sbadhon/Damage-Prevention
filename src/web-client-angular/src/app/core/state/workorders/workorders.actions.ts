import { createAction, props } from '@ngrx/store';
import { WorkOrder, WorkOrderStatus, Crew } from './workorders.models';
import { ListParams, PagedResponse } from '../util/util.model';

// Load WorkOrders
export const loadWorkOrders = createAction(
  '[WorkOrders] Load',
  props<{ params: ListParams }>()
);

export const loadWorkOrdersSuccess = createAction(
  '[WorkOrders] Load Success',
  props<{ workOrders: PagedResponse<WorkOrder> }>()
);

export const loadWorkOrdersFailure = createAction(
  '[WorkOrders] Load Failure',
  props<{ error: unknown }>()
);

// Update WorkOrder Status
export const updateWorkOrderStatus = createAction(
  '[WorkOrders] Update Status',
  props<{ id: string; status: WorkOrderStatus }>()
);

export const updateWorkOrderStatusSuccess = createAction(
  '[WorkOrders] Update Status Success',
  props<{ workOrder: WorkOrder }>()
);

export const updateWorkOrderStatusFailure = createAction(
  '[WorkOrders] Update Status Failure',
  props<{ error: unknown }>()
);

// Assign Crew
export const assignCrew = createAction(
  '[WorkOrders] Assign Crew',
  props<{ workOrderId: string; crewId: string }>()
);

export const assignCrewSuccess = createAction(
  '[WorkOrders] Assign Crew Success',
  props<{ workOrder: WorkOrder }>()
);

export const assignCrewFailure = createAction(
  '[WorkOrders] Assign Crew Failure',
  props<{ error: unknown }>()
);

// Load Crews
export const loadCrews = createAction('[WorkOrders] Load Crews');

export const loadCrewsSuccess = createAction(
  '[WorkOrders] Load Crews Success',
  props<{ crews: Crew[] }>()
);

export const loadCrewsFailure = createAction(
  '[WorkOrders] Load Crews Failure',
  props<{ error: unknown }>()
);

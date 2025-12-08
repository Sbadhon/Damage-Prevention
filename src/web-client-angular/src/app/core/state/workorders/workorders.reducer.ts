import { createReducer, on } from '@ngrx/store';
import * as WorkOrderActions from './workorders.actions';
import { WorkOrder, Crew } from './workorders.models';
import { PagedResponse } from '../util/util.model';

export const WORKORDERS_FEATURE_KEY = 'workorders';

export interface WorkOrderState {
  workOrders: PagedResponse<WorkOrder> | null;
  crews: Crew[];
  loading: boolean;
  error: unknown;
}

export const initialState: WorkOrderState = {
  workOrders: null,
  crews: [],
  loading: false,
  error: null,
};

export const workOrderReducer = createReducer(
  initialState,

  // Load WorkOrders
  on(WorkOrderActions.loadWorkOrders, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(WorkOrderActions.loadWorkOrdersSuccess, (state, { workOrders }) => ({
    ...state,
    workOrders,
    loading: false,
  })),
  on(WorkOrderActions.loadWorkOrdersFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Update Status
  on(WorkOrderActions.updateWorkOrderStatusSuccess, (state, { workOrder }) => ({
    ...state,
    workOrders: state.workOrders
      ? {
          ...state.workOrders,
          items: state.workOrders.items.map((wo) =>
            wo.workOrderId === workOrder.workOrderId ? workOrder : wo
          ),
        }
      : { items: [workOrder], totalCount: 1, pageNumber: 1, pageSize: 10 },
  })),
  on(WorkOrderActions.updateWorkOrderStatusFailure, (state, { error }) => ({
    ...state,
    error,
  })),

  // Assign Crew
  on(WorkOrderActions.assignCrewSuccess, (state, { workOrder }) => ({
    ...state,
    workOrders: state.workOrders
      ? {
          ...state.workOrders,
          items: state.workOrders.items.map((wo) =>
            wo.workOrderId === workOrder.workOrderId ? workOrder : wo
          ),
        }
      : { items: [workOrder], totalCount: 1, pageNumber: 1, pageSize: 10 },
  })),
  on(WorkOrderActions.assignCrewFailure, (state, { error }) => ({
    ...state,
    error,
  })),

  // Load Crews
  on(WorkOrderActions.loadCrewsSuccess, (state, { crews }) => ({
    ...state,
    crews,
  })),
  on(WorkOrderActions.loadCrewsFailure, (state, { error }) => ({
    ...state,
    error,
  }))
);

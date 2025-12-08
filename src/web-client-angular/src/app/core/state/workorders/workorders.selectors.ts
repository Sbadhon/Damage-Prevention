import { createFeatureSelector, createSelector } from '@ngrx/store';
import { WORKORDERS_FEATURE_KEY, WorkOrderState } from './workorders.reducer';

export const selectWorkOrdersState =
  createFeatureSelector<WorkOrderState>(WORKORDERS_FEATURE_KEY);

export const selectWorkOrders = createSelector(
  selectWorkOrdersState,
  (state) => state.workOrders?.items || []
);

export const selectWorkOrdersPaged = createSelector(
  selectWorkOrdersState,
  (state) => state.workOrders
);

export const selectWorkOrdersLoading = createSelector(
  selectWorkOrdersState,
  (state) => state.loading
);

export const selectWorkOrdersError = createSelector(
  selectWorkOrdersState,
  (state) => state.error
);

export const selectCrews = createSelector(
  selectWorkOrdersState,
  (state) => state.crews
);

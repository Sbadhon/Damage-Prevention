import { createAsyncThunk, createSlice, PayloadAction } from "@reduxjs/toolkit";
import { WorkOrder, WorkOrderStatus, PagedResponse } from "../types";
import api from "../services/api";
import type { RootState } from "../store";

export interface WorkOrdersState {
  items: WorkOrder[];
  loading: boolean;
  error?: string;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

const initialState: WorkOrdersState = {
  items: [],
  loading: false,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
};

export const fetchWorkOrdersPage = createAsyncThunk<
  PagedResponse<WorkOrder>,
  { pageNumber: number; pageSize?: number }
>("workOrders/fetchPage", async ({ pageNumber, pageSize }, thunkApi) => {
  const state = thunkApi.getState() as RootState;
  const effectivePageSize = pageSize ?? state.workOrders.pageSize;
  const response = await api.listWorkOrders({ pageNumber, pageSize: effectivePageSize });
  return response;
});

export const updateWorkOrderStatusThunk = createAsyncThunk<
  WorkOrder,
  { workOrderId: string; newStatus: WorkOrderStatus }
>("workOrders/updateStatus", async ({ workOrderId, newStatus }) => {
  const response = await api.updateWorkOrderStatus(workOrderId, newStatus);
  return response;
});

// --- Assign Crew Thunk ---
export const assignCrewThunk = createAsyncThunk<
  WorkOrder,
  { workOrderId: string; crewId: string }
>("workOrders/assignCrew", async ({ workOrderId, crewId }) => {
  const response = await api.assignCrew(workOrderId, crewId);
  return response;
});

const workOrdersSlice = createSlice({
  name: "workOrders",
  initialState,
  reducers: {
    setPage(state, action: PayloadAction<number>) {
      state.pageNumber = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchWorkOrdersPage.pending, (state) => {
        state.loading = true;
        state.error = undefined;
      })
      .addCase(fetchWorkOrdersPage.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload.items;
        state.totalCount = action.payload.totalCount;
        state.pageNumber = action.payload.pageNumber;
        state.pageSize = action.payload.pageSize;
      })
      .addCase(fetchWorkOrdersPage.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? "Failed to load work orders";
      })
      .addCase(updateWorkOrderStatusThunk.fulfilled, (state, action) => {
        const updatedWO = action.payload;
        const index = state.items.findIndex((w) => w.workOrderId === updatedWO.workOrderId);
        if (index !== -1) state.items[index] = updatedWO;
      })
      .addCase(assignCrewThunk.fulfilled, (state, action) => {
        const updatedWO = action.payload;
        const index = state.items.findIndex((w) => w.workOrderId === updatedWO.workOrderId);
        if (index !== -1) state.items[index] = updatedWO;
      });
  },
});

export const { setPage } = workOrdersSlice.actions;
export const selectWorkOrdersState = (state: RootState) => state.workOrders;
export default workOrdersSlice.reducer;

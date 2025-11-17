import { createAsyncThunk, createSlice } from '@reduxjs/toolkit';
import { RiskAssessment, PagedResponse } from '../types';
import api from '../services/api';
import type { RootState } from '../store';

export interface RiskState {
  items: RiskAssessment[];
  loading: boolean;
  error?: string;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

const initialState: RiskState = {
  items: [],
  loading: false,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
};

export const fetchRiskPage = createAsyncThunk<
  PagedResponse<RiskAssessment>,
  { pageNumber: number; pageSize?: number }
>('risk/fetchPage', async ({ pageNumber, pageSize }, thunkApi) => {
  const state = thunkApi.getState() as RootState;
  const effectivePageSize = pageSize ?? state.risk.pageSize;

  const response = await api.listRiskAssessments({
    pageNumber,
    pageSize: effectivePageSize,
  });

  return response;
});

const riskSlice = createSlice({
  name: 'risk',
  initialState,
  reducers: {
    setPage(state, action) {
      state.pageNumber = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchRiskPage.pending, (state) => {
        state.loading = true;
        state.error = undefined;
      })
      .addCase(fetchRiskPage.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload.items;
        state.totalCount = action.payload.totalCount;
        state.pageNumber = action.payload.pageNumber;
        state.pageSize = action.payload.pageSize;
      })
      .addCase(fetchRiskPage.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to load risk data';
      });
  },
});

export const { setPage } = riskSlice.actions;

export const selectRiskState = (state: RootState) => state.risk;

export default riskSlice.reducer;

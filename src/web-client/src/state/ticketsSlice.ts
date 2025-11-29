import { createAsyncThunk, createSlice, PayloadAction } from '@reduxjs/toolkit';
import { Ticket, TicketStatus, DashboardStats, PagedResponse } from '../types';
import api from '../services/api';
import type { RootState } from '../store';

export interface TicketsState {
  items: Ticket[];
  loading: boolean;
  error?: string;
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  stats?: DashboardStats;
}

const initialState: TicketsState = {
  items: [],
  loading: false,
  pageNumber: 1,
  pageSize: 10,
  totalCount: 0,
  stats: undefined,
};

// ---- Thunks ----

export const fetchTicketsPage = createAsyncThunk<
  PagedResponse<Ticket>,
  { pageNumber: number; pageSize?: number }
>('tickets/fetchPage', async ({ pageNumber, pageSize }, thunkApi) => {
  const state = thunkApi.getState() as RootState;
  const effectivePageSize = pageSize ?? state.tickets?.pageSize;

  const response = await api.listTickets({
    pageNumber,
    pageSize: effectivePageSize,
  });

  return response;
});

export const updateTicketStatusThunk = createAsyncThunk<
  { ticketId: string; newStatus: TicketStatus },
  { ticketId: string; newStatus: TicketStatus }
>('tickets/updateStatus', async ({ ticketId, newStatus }) => {
  await api.updateTicketStatus(ticketId, newStatus);
  return { ticketId, newStatus };
});

export const createTicketThunk = createAsyncThunk<
  Ticket,
  Omit<Ticket, 'ticketId' | 'status' | 'createdAt'>
>('tickets/create', async (newTicketData) => {
  const created = await api.createTicket(newTicketData);
  return created;
});

// ---- Slice ----

const ticketsSlice = createSlice({
  name: 'tickets',
  initialState,
  reducers: {
    setPage(state, action: PayloadAction<number>) {
      state.pageNumber = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder
      // fetchTicketsPage
      .addCase(fetchTicketsPage.pending, (state) => {
        state.loading = true;
        state.error = undefined;
      })
      .addCase(fetchTicketsPage.fulfilled, (state, action) => {
        state.loading = false;
        state.items = action.payload.items;
        state.totalCount = action.payload.totalCount;
        state.pageNumber = action.payload.pageNumber;
        state.pageSize = action.payload.pageSize;

        // Compute ticket stats only
        state.stats = calculateTicketStats(state.items);
      })
      .addCase(fetchTicketsPage.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message ?? 'Failed to load tickets';
      })

      // updateTicketStatusThunk
      .addCase(updateTicketStatusThunk.fulfilled, (state, action) => {
        const { ticketId, newStatus } = action.payload;
        const ticket = state.items.find((t) => t.ticketId === ticketId);
        if (ticket) {
          ticket.status = newStatus;
        }

        // Recalculate ticket stats
        state.stats = calculateTicketStats(state.items);
      })

      // createTicketThunk
      .addCase(createTicketThunk.fulfilled, (state, action) => {
        state.items = [action.payload, ...state.items];
        state.totalCount += 1;

        // Recalculate ticket stats
        state.stats = calculateTicketStats(state.items);
      });
  },
});

// ---- Helpers ----

function calculateTicketStats(items: Ticket[]): DashboardStats {
  const ticketsStats = { open: 0, inProgress: 0, completed: 0 };

  for (const t of items) {
    switch (t.status) {
      case TicketStatus.Open:
        ticketsStats.open += 1;
        break;
      case TicketStatus.InProgress:
        ticketsStats.inProgress += 1;
        break;
      case TicketStatus.Completed:
        ticketsStats.completed += 1;
        break;
    }
  }

  // Provide default empty stats for workOrders and risks
  return {
    tickets: ticketsStats,
    workOrders: { pending: 0, assigned: 0, completed: 0 },
    risks: { high: 0, critical: 0 },
  };
}


// ---- Exports ----

export const { setPage } = ticketsSlice.actions;
export const selectTicketsState = (state: RootState) => state.tickets;
export default ticketsSlice.reducer;

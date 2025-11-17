import { configureStore } from '@reduxjs/toolkit';
import ticketsReducer from './state/ticketsSlice';
import workOrdersReducer from './state/workOrdersSlice';
import riskReducer from './state/riskSlice';

export const store = configureStore({
  reducer: {
    tickets: ticketsReducer,
    workOrders: workOrdersReducer,
    risk: riskReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;

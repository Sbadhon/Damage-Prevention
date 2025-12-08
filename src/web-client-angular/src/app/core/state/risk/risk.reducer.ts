import { createReducer, on } from '@ngrx/store';
import * as RiskActions from './risk.actions';
import { RiskAssessment } from './risk.models';
import { PagedResponse } from '../util/util.model';

export const RISK_FEATURE_KEY = 'risk';

export interface RiskState {
  risks: PagedResponse<RiskAssessment> | null;
  selectedRisk: RiskAssessment | null;
  loading: boolean;
  error: unknown;
}

export const initialState: RiskState = {
  risks: null,
  selectedRisk: null,
  loading: false,
  error: null,
};

export const riskReducer = createReducer(
  initialState,

  // Load Risk Assessments
  on(RiskActions.loadRiskAssessments, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(RiskActions.loadRiskAssessmentsSuccess, (state, { risks }) => ({
    ...state,
    risks,
    loading: false,
  })),
  on(RiskActions.loadRiskAssessmentsFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  // Load Risk by Ticket
  on(RiskActions.loadRiskByTicket, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),
  on(RiskActions.loadRiskByTicketSuccess, (state, { risk }) => ({
    ...state,
    selectedRisk: risk,
    loading: false,
  })),
  on(RiskActions.loadRiskByTicketFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  }))
);

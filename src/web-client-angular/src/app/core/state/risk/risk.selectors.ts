import { createFeatureSelector, createSelector } from '@ngrx/store';
import { RISK_FEATURE_KEY, RiskState } from './risk.reducer';

export const selectRiskState = createFeatureSelector<RiskState>(RISK_FEATURE_KEY);

export const selectRiskAssessments = createSelector(
  selectRiskState,
  (state) => state.risks?.items || [],
);

export const selectRiskAssessmentsPaged = createSelector(selectRiskState, (state) => state.risks);

export const selectSelectedRisk = createSelector(selectRiskState, (state) => state.selectedRisk);

export const selectRiskLoading = createSelector(selectRiskState, (state) => state.loading);

export const selectRiskError = createSelector(selectRiskState, (state) => state.error);

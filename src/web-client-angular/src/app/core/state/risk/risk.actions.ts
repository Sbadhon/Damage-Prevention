import { createAction, props } from '@ngrx/store';
import { RiskAssessment } from './risk.models';
import { ListParams, PagedResponse } from '../util/util.model';

// Load Risk Assessments
export const loadRiskAssessments = createAction(
  '[Risk] Load',
  props<{ params: ListParams }>()
);

export const loadRiskAssessmentsSuccess = createAction(
  '[Risk] Load Success',
  props<{ risks: PagedResponse<RiskAssessment> }>()
);

export const loadRiskAssessmentsFailure = createAction(
  '[Risk] Load Failure',
  props<{ error: unknown }>()
);

// Load Risk by Ticket
export const loadRiskByTicket = createAction(
  '[Risk] Load By Ticket',
  props<{ ticketId: string }>()
);

export const loadRiskByTicketSuccess = createAction(
  '[Risk] Load By Ticket Success',
  props<{ risk: RiskAssessment }>()
);

export const loadRiskByTicketFailure = createAction(
  '[Risk] Load By Ticket Failure',
  props<{ error: unknown }>()
);

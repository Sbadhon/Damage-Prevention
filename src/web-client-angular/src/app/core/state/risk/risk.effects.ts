import { Injectable, inject } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import * as RiskActions from './risk.actions';
import { RiskService } from './risk.api';
import { mergeMap, map, catchError, switchMap, of } from 'rxjs';

@Injectable()
export class RiskEffects {
  private readonly actions$ = inject(Actions);
  private readonly api = inject(RiskService);

  loadRiskAssessments$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RiskActions.loadRiskAssessments),
      switchMap(({ params }) =>
        this.api.listRiskAssessments(params).pipe(
          map((risks) => RiskActions.loadRiskAssessmentsSuccess({ risks })),
          catchError((error) => of(RiskActions.loadRiskAssessmentsFailure({ error })))
        )
      )
    )
  );

  loadRiskByTicket$ = createEffect(() =>
    this.actions$.pipe(
      ofType(RiskActions.loadRiskByTicket),
      mergeMap(({ ticketId }) =>
        this.api.getRiskByTicketId(ticketId).pipe(
          map((risk) => RiskActions.loadRiskByTicketSuccess({ risk })),
          catchError((error) => of(RiskActions.loadRiskByTicketFailure({ error })))
        )
      )
    )
  );
}

import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { map, takeUntil } from 'rxjs/operators';

import * as RiskActions from '@app/core/state/risk/risk.actions';
import * as RiskSelectors from '@app/core/state/risk/risk.selectors';
import { RiskAssessment, RiskLevel } from '@app/core/state/risk/risk.models';
import { ListParams } from '@app/core/state/util/util.model';

import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import {
  DashboardSummary,
  DashboardStat,
} from '@app/shared/component/dashboard-summary/dashboard-summary';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';

@Component({
  selector: 'dp-risk-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    DashboardSummary,
    StatusBadge,
  ],
  templateUrl: './risk-dashboard.html',
  styleUrls: ['./risk-dashboard.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RiskDashboard implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly destroy$ = new Subject<void>();

  readonly riskState$ = this.store.select(RiskSelectors.selectRiskState);

  readonly displayedColumns: string[] = [
    'riskId',
    'ticketId',
    'level',
    'score',
    'address',
    'assessedAt',
  ];

  readonly dashboardStats$ = this.riskState$.pipe(
    map((state) => this.buildDashboardStats(state.risks?.items ?? [])),
  );

  ngOnInit(): void {
    const params: ListParams = {
      pageNumber: 1,
      pageSize: 10,
    };
    this.store.dispatch(RiskActions.loadRiskAssessments({ params }));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  handlePageChange(event: PageEvent): void {
    const params: ListParams = {
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    };
    this.store.dispatch(RiskActions.loadRiskAssessments({ params }));
  }

  trackByRiskId(_index: number, risk: RiskAssessment): string {
    return risk.riskId;
  }

  private buildDashboardStats(risks: RiskAssessment[]): DashboardStat[] {
    const high = risks.filter((r) => r.level === RiskLevel.High).length;
    const critical = risks.filter((r) => r.level === RiskLevel.Critical).length;
    const medium = risks.filter((r) => r.level === RiskLevel.Medium).length;
    const low = risks.filter((r) => r.level === RiskLevel.Low).length;

    return [
      {
        title: 'High Risk',
        value: high,
        color: 'bg-orange-100 dark:bg-orange-500/20',
      },
      {
        title: 'Critical Risk',
        value: critical,
        color: 'bg-red-100 dark:bg-red-500/20',
      },
      {
        title: 'Medium Risk',
        value: medium,
        color: 'bg-yellow-100 dark:bg-yellow-500/20',
      },
      {
        title: 'Low Risk',
        value: low,
        color: 'bg-green-100 dark:bg-green-500/20',
      },
    ];
  }
}

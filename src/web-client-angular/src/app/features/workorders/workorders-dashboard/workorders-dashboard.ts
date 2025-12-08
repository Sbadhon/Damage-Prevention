import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import {
  DashboardSummary,
  DashboardStat,
} from '@app/shared/component/dashboard-summary/dashboard-summary';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';
import { WorkOrderDetail, WorkOrderDetailData } from '../workorder-details/workorder-details';
import { WorkOrder, WorkOrderStatus } from '@app/core/state/workorders/workorders.models';
import * as WorkOrderSelectors from '@app/core/state/workorders/workorders.selectors';
import * as WorkOrderActions from '@app/core/state/workorders/workorders.actions';
import { ListParams } from '@app/core/state/util/util.model';

@Component({
  selector: 'app-workorder-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSelectModule,
    MatInputModule,
    MatFormFieldModule,
    DashboardSummary,
    StatusBadge,
  ],
  templateUrl: './workorders-dashboard.html',
  styleUrls: ['./workorders-dashboard.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkordersDashboard implements OnInit, OnDestroy {
  WorkOrderStatus = WorkOrderStatus;
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  readonly workOrdersState$ = this.store.select(WorkOrderSelectors.selectWorkOrdersState);

  readonly statusOptions: Array<WorkOrderStatus | 'All'> = [
    'All',
    ...Object.values(WorkOrderStatus),
  ];
  statusFilter: WorkOrderStatus | 'All' = 'All';
  searchTerm = '';

  readonly displayedColumns: string[] = [
    'workOrderId',
    'ticketId',
    'crew',
    'scheduledAt',
    'status',
    'actions',
  ];

  get filteredStatusOptions(): WorkOrderStatus[] {
    return this.statusOptions.filter((status): status is WorkOrderStatus => status !== 'All');
  }

  ngOnInit(): void {
    this.loadPage({ pageNumber: 1, pageSize: 10 });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadPage(params: ListParams): void {
    this.store.dispatch(WorkOrderActions.loadWorkOrders({ params }));
  }

  handlePageChange(event: PageEvent): void {
    this.loadPage({ pageNumber: event.pageIndex + 1, pageSize: event.pageSize });
  }

  openWorkOrderDetails(workOrder: WorkOrder): void {
    const dialogRef = this.dialog.open<WorkOrderDetail, WorkOrderDetailData, any>(WorkOrderDetail, {
      width: '900px',
      data: { workOrder },
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        if (result?.statusChanged && result.newStatus) {
          this.store.dispatch(
            WorkOrderActions.updateWorkOrderStatus({
              id: workOrder.workOrderId,
              status: result.newStatus,
            }),
          );
        }
      });
  }

  filteredWorkOrders(workOrders: WorkOrder[] | null | undefined): WorkOrder[] {
    if (!workOrders?.length) return [];

    const term = this.searchTerm.trim().toLowerCase();

    return workOrders.filter((w) => {
      const matchesStatus = this.statusFilter === 'All' ? true : w.status === this.statusFilter;
      const matchesSearch =
        !term ||
        w.workOrderId.toLowerCase().includes(term) ||
        w.ticketId.toLowerCase().includes(term) ||
        (w.crewName ?? '').toLowerCase().includes(term) ||
        w.details.toLowerCase().includes(term);
      return matchesStatus && matchesSearch;
    });
  }

  readonly dashboardStats$ = this.workOrdersState$.pipe(
    map((state) => this.buildDashboardStats(state?.workOrders?.items ?? [])),
  );

  private buildDashboardStats(workOrders: WorkOrder[]): DashboardStat[] {
    return [
      {
        title: 'Pending',
        value: workOrders.filter((w) => w.status === WorkOrderStatus.Pending).length,
        color: 'bg-blue-100 dark:bg-blue-900/40',
      },
      {
        title: 'Assigned',
        value: workOrders.filter((w) => w.status === WorkOrderStatus.Assigned).length,
        color: 'bg-yellow-100 dark:bg-yellow-500/20',
      },
      {
        title: 'Completed',
        value: workOrders.filter((w) => w.status === WorkOrderStatus.Completed).length,
        color: 'bg-green-100 dark:bg-green-500/20',
      },
      {
        title: 'Cancelled',
        value: workOrders.filter((w) => w.status === WorkOrderStatus.Cancelled).length,
        color: 'bg-red-100 dark:bg-red-500/20',
      },
    ];
  }

  getAllowedTransitions(current: WorkOrderStatus): WorkOrderStatus[] {
    switch (current) {
      case WorkOrderStatus.Pending:
        return [WorkOrderStatus.Pending, WorkOrderStatus.Assigned, WorkOrderStatus.Cancelled];
      case WorkOrderStatus.Assigned:
        return [WorkOrderStatus.Assigned, WorkOrderStatus.Completed, WorkOrderStatus.Cancelled];
      case WorkOrderStatus.Completed:
        return [WorkOrderStatus.Completed];
      case WorkOrderStatus.Cancelled:
        return [WorkOrderStatus.Cancelled];
      default:
        return [];
    }
  }

  handleStatusChange(id: string, newStatus: WorkOrderStatus): void {
    this.store.dispatch(
      WorkOrderActions.updateWorkOrderStatus({
        id,
        status: newStatus,
      }),
    );
  }
  onStatusChange(event: Event, workOrderId: string): void {
    const value = (event.target as HTMLSelectElement).value as WorkOrderStatus;
    this.handleStatusChange(workOrderId, value);
  }
}

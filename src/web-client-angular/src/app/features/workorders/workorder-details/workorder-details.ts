import {
  ChangeDetectionStrategy,
  Component,
  inject,
  Inject,
  OnDestroy,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';
import * as WorkOrderSelectors from '@app/core/state/workorders/workorders.selectors';
import * as WorkOrderActions from '@app/core/state/workorders/workorders.actions';
import { Crew, WorkOrder, WorkOrderStatus } from '@app/core/state/workorders/workorders.models';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';

export interface WorkOrderDetailData {
  workOrder: WorkOrder;
}

@Component({
  standalone: true,
  selector: 'dp-workorder-detail',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    StatusBadge,
  ],
  templateUrl: './workorder-details.html',
  styleUrls: ['./workorder-details.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WorkOrderDetail implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly destroy$ = new Subject<void>();

  workOrder: WorkOrder;
  currentStatus: WorkOrderStatus;

  selectedCrew: string = '';
  crews: Crew[] = [];
  loadingCrews = false;
  assigning = false;
  isEditable = true;

  constructor(
    @Inject(MAT_DIALOG_DATA) data: WorkOrderDetailData,
    private readonly dialogRef: MatDialogRef<WorkOrderDetail>
  ) {
    this.workOrder = data.workOrder;
    this.currentStatus = data.workOrder.status;
    this.selectedCrew = data.workOrder.crewName || '';
  }

  ngOnInit(): void {
    this.store.dispatch(WorkOrderActions.loadCrews());
    this.store
      .select(WorkOrderSelectors.selectCrews)
      .pipe(takeUntil(this.destroy$))
      .subscribe((crews) => (this.crews = crews));
  }

  assignCrew(): void {
    if (!this.selectedCrew || this.selectedCrew === this.workOrder.crewName) return;

    const crew = this.crews.find((c) => c.crewName === this.selectedCrew);
    if (!crew) return;

    this.assigning = true;
    this.store.dispatch(
      WorkOrderActions.assignCrew({
        workOrderId: this.workOrder.workOrderId,
        crewId: crew.crewId,
      })
    );
    this.dialogRef.close({ assigned: true, crewId: crew.crewId });
  }

  close(): void {
    this.dialogRef.close({ assigned: false });
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
        return [current];
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

import {
  ChangeDetectionStrategy,
  Component,
  inject,
  Inject,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormField, MatSelect, MatOption } from '@angular/material/select';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';

import { Ticket, TicketStatus } from '@app/core/state/ticket/ticket.models';
import { updateTicketStatus } from '@app/core/state/ticket/ticket.actions';

import * as RiskActions from '@app/core/state/risk/risk.actions';
import * as RiskSelectors from '@app/core/state/risk/risk.selectors';
import { Subject } from 'rxjs';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';

export interface TicketDetailData {
  ticket: Ticket;
}

@Component({
  standalone: true,
  selector: 'dp-ticket-detail',
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormField,
    MatSelect,
    MatOption,
    StatusBadge,
  ],
  templateUrl: './ticket-detail.html',
  styleUrls: ['./ticket-detail.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TicketDetail implements OnInit {
  private readonly store = inject(Store);
  private readonly destroy$ = new Subject<void>();
  
  ticket: Ticket;
  selectedStatus!: TicketStatus;
  statusOptions: TicketStatus[] = [];
  risk$ = this.store.select(RiskSelectors.selectSelectedRisk);
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: TicketDetailData,
    private readonly dialogRef: MatDialogRef<TicketDetail>,
  ) {
    this.ticket = data.ticket;
    this.selectedStatus = data.ticket.status;
  }

  ngOnInit() {
    this.store.dispatch(
      RiskActions.loadRiskByTicket({ ticketId: this.ticket.ticketId })
    );

    this.statusOptions = this.getAllowedTransitions(
      this.ticket.status,
      this.ticket.crewId
    );
  }

  getAllowedTransitions(
    current: TicketStatus,
    crewAssigned?: string | null
  ): TicketStatus[] {
    switch (current) {
      case TicketStatus.Open: {
        const transitions = [
          TicketStatus.Open,
          TicketStatus.Completed,
          TicketStatus.Cancelled,
        ];

        if (crewAssigned) {
          transitions.splice(1, 0, TicketStatus.InProgress);
        }
        return transitions;
      }

      case TicketStatus.InProgress:
        return [
          TicketStatus.InProgress,
          TicketStatus.Completed,
          TicketStatus.Cancelled,
        ];

      case TicketStatus.Completed:
      case TicketStatus.Cancelled:
        return [current];

      default:
        return [current];
    }
  }

  onStatusChange(newStatus: TicketStatus) {
    this.selectedStatus = newStatus;

    this.store.dispatch(
      updateTicketStatus({
        ticketId: this.ticket.ticketId,
        newStatus,
      })
    );
  }

  close(): void {
    this.dialogRef.close();
  }
}

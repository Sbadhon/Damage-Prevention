import { ChangeDetectionStrategy, Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatOptionModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { FormsModule } from '@angular/forms';
import { Ticket, TicketStatus } from '@app/core/state/ticket/ticket.models';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';
import { RiskService } from '@app/core/state/risk/risk.api';
import { RiskAssessment } from '@app/core/state/risk/risk.models';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

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
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatButtonModule,
    StatusBadge,
  ],
  templateUrl: './ticket-detail.html',
  styleUrls: ['./ticket-detail.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TicketDetail implements OnInit {
  ticket: Ticket;
  currentStatus: TicketStatus;
  risk$: Observable<RiskAssessment | null> = of(null);

  constructor(
    @Inject(MAT_DIALOG_DATA) data: TicketDetailData,
    private readonly dialogRef: MatDialogRef<TicketDetail>,
    private readonly riskService: RiskService,
  ) {
    this.ticket = data.ticket;
    this.currentStatus = data.ticket.status;
  }

  ngOnInit(): void {
    this.risk$ = this.riskService
      .getRiskByTicketId(this.ticket.ticketId)
      .pipe(catchError(() => of(null)));
  }

  close(): void {
    this.dialogRef.close({ statusChanged: false });
  }

  saveStatus(): void {
    if (this.currentStatus !== this.ticket.status) {
      this.dialogRef.close({
        statusChanged: true,
        newStatus: this.currentStatus,
      });
    } else {
      this.dialogRef.close({ statusChanged: false });
    }
  }

  getAllowedTransitions(current: TicketStatus): TicketStatus[] {
    switch (current) {
      case TicketStatus.Open:
        return [TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Cancelled];
      case TicketStatus.InProgress:
        return [TicketStatus.InProgress, TicketStatus.Completed, TicketStatus.Cancelled];
      case TicketStatus.Completed:
        return [TicketStatus.Completed];
      case TicketStatus.Cancelled:
        return [TicketStatus.Cancelled];
      default:
        return [current];
    }
  }
}

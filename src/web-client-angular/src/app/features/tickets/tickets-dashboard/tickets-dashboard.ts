import { ChangeDetectionStrategy, Component, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import * as TicketActions from '@app/core/state/ticket/ticket.actions';
import * as TicketSelectors from '@app/core/state/ticket/ticket.selectors';
import { Ticket, TicketStatus } from '@app/core/state/ticket/ticket.models';
import { ListParams } from '@app/core/state/util/util.model';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TicketForm } from '../ticket-form/ticket-form';
import { StatusBadge } from '@app/shared/component/status-badge/status-badge';
import {
  DashboardSummary,
  DashboardStat,
} from '@app/shared/component/dashboard-summary/dashboard-summary';
import { map } from 'rxjs/operators';
import { TicketDetail, TicketDetailData } from '@app/features/tickets/ticket-detail/ticket-detail';

@Component({
  selector: 'app-ticket-dashboard',
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
    StatusBadge,
    DashboardSummary,
  ],
  templateUrl: './tickets-dashboard.html',
  styleUrls: ['./tickets-dashboard.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TicketsDashboard implements OnInit, OnDestroy {
  private readonly store = inject(Store);
  private readonly dialog = inject(MatDialog);
  private readonly destroy$ = new Subject<void>();

  readonly ticketsState$ = this.store.select(TicketSelectors.selectTicketsState);

  readonly statusOptions: Array<TicketStatus | 'All'> = ['All', ...Object.values(TicketStatus)];
  statusFilter: TicketStatus | 'All' = 'All';
  searchTerm = '';

  readonly displayedColumns: string[] = [
    'ticketId',
    'description',
    'workType',
    'address',
    'status',
    'actions',
  ];

  get filteredStatusOptions(): TicketStatus[] {
    return this.statusOptions.filter((status): status is TicketStatus => status !== 'All');
  }

  ngOnInit(): void {
    const params: ListParams = {
      pageNumber: 1,
      pageSize: 10,
    };
    this.store.dispatch(TicketActions.loadTickets({ params }));
  }

  openAddTicket(): void {
    const dialogRef = this.dialog.open(TicketForm, {
      width: '640px',
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        if (result) {
          this.store.dispatch(TicketActions.createTicket({ ticket: result }));
        }
      });
  }

  openTicketDetails(ticket: Ticket): void {
    const dialogRef = this.dialog.open<TicketDetail, TicketDetailData, any>(TicketDetail, {
      width: '900px',
      data: { ticket },
    });

    dialogRef
      .afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((result) => {
        if (result?.statusChanged && result.newStatus) {
          this.store.dispatch(
            TicketActions.updateTicketStatus({
              ticketId: ticket.ticketId,
              newStatus: result.newStatus,
            }),
          );
        }
      });
  }

  handleStatusChange(ticketId: string, newStatus: TicketStatus): void {
    this.store.dispatch(TicketActions.updateTicketStatus({ ticketId, newStatus }));
  }

  handlePageChange(event: PageEvent): void {
    const params: ListParams = {
      pageNumber: event.pageIndex + 1,
      pageSize: event.pageSize,
    };
    this.store.dispatch(TicketActions.loadTickets({ params }));
  }

  filteredTickets(tickets: Ticket[] | null | undefined): Ticket[] {
    if (!tickets?.length) {
      return [];
    }

    const term = this.searchTerm.trim().toLowerCase();

    return tickets.filter((t) => {
      const matchesStatus = this.statusFilter === 'All' ? true : t.status === this.statusFilter;

      const matchesSearch =
        !term ||
        t.ticketId.toLowerCase().includes(term) ||
        t.description.toLowerCase().includes(term) ||
        t.address.toLowerCase().includes(term);

      return matchesStatus && matchesSearch;
    });
  }

  trackByTicketId(_index: number, ticket: Ticket): string {
    return ticket.ticketId;
  }

  readonly dashboardStats$ = this.ticketsState$.pipe(
    map((state) => this.buildDashboardStats(state.tickets?.items ?? [])),
  );

  private buildDashboardStats(tickets: Ticket[]): DashboardStat[] {
    const open = tickets.filter((t) => t.status === TicketStatus.Open).length;
    const inProgress = tickets.filter((t) => t.status === TicketStatus.InProgress).length;
    const completed = tickets.filter((t) => t.status === TicketStatus.Completed).length;
    //  wire RiskState in later.
    const highRisk = 0;

    return [
      {
        title: 'Open Tickets',
        value: open,
        color: 'bg-blue-100 dark:bg-blue-900/40',
      },
      {
        title: 'In Progress',
        value: inProgress,
        color: 'bg-yellow-100 dark:bg-yellow-500/20',
      },
      {
        title: 'Completed',
        value: completed,
        color: 'bg-green-100 dark:bg-green-500/20',
      },
      {
        title: 'High/Critical Risk',
        value: highRisk,
        color: 'bg-red-100 dark:bg-red-500/20',
      },
    ];
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}

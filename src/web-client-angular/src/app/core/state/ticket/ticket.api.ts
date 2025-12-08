import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';
import { ListParams, PagedResponse } from '../util/util.model';
import { Ticket, TicketStatus } from './ticket.models';

@Injectable({
  providedIn: 'root',
})
export class TicketService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBase;
  private readonly tenantId = environment.tenantId || 'acme-corp';

  private get headers() {
    return { 'X-Tenant-Id': this.tenantId };
  }

  listTickets(params: ListParams): Observable<PagedResponse<Ticket>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);

    return this.http.get<PagedResponse<Ticket>>(`${this.apiBase}/api/tickets`, {
      params: httpParams,
      headers: this.headers,
    });
  }

  getTicketById(id: string): Observable<Ticket> {
    return this.http.get<Ticket>(`${this.apiBase}/api/tickets/${id}`, {
      headers: this.headers,
    });
  }

  createTicket(
    newTicket: Omit<Ticket, 'ticketId' | 'status' | 'createdAt'>
  ): Observable<Ticket> {
    const body = {
      workType: newTicket.workType,
      address: newTicket.address,
      description: newTicket.description,
      lat: newTicket.lat ?? 0,
      lon: newTicket.lon ?? 0,
    };

    return this.http.post<Ticket>(`${this.apiBase}/api/tickets`, body, {
      headers: this.headers,
    });
  }

  updateTicketStatus(
    ticketId: string,
    newStatus: TicketStatus,
    reason?: string
  ): Observable<void> {
    switch (newStatus) {
      case TicketStatus.Completed:
        return this.http.post<void>(
          `${this.apiBase}/api/tickets/${ticketId}/complete`,
          null,
          { headers: this.headers }
        );

      case TicketStatus.Cancelled:
        return this.http.post<void>(
          `${this.apiBase}/api/tickets/${ticketId}/cancel`,
          reason ? { reason } : null,
          { headers: this.headers }
        );

      default:
        return this.http.patch<void>(
          `${this.apiBase}/api/tickets/${ticketId}`,
          { status: newStatus },
          { headers: this.headers }
        );
    }
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';
import { Crew, WorkOrder, WorkOrderStatus } from './workorders.models';
import { ListParams, PagedResponse } from '../util/util.model';

@Injectable({
  providedIn: 'root',
})
export class WorkOrderService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBase;
  private readonly tenantId = environment.tenantId || 'acme-corp';

  private get headers() {
    return { 'X-Tenant-Id': this.tenantId };
  }

  listWorkOrders(params: ListParams): Observable<PagedResponse<WorkOrder>> {
    let httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);

    return this.http.get<PagedResponse<WorkOrder>>(
      `${this.apiBase}/api/workorders`,
      { params: httpParams, headers: this.headers }
    );
  }

  updateWorkOrderStatus(
    workOrderId: string,
    newStatus: WorkOrderStatus
  ): Observable<WorkOrder> {
    return this.http.patch<WorkOrder>(
      `${this.apiBase}/api/workorders/${workOrderId}/status`,
      { status: newStatus },
      { headers: this.headers }
    );
  }

  listCrews(): Observable<Crew[]> {
    return this.http.get<Crew[]>(`${this.apiBase}/api/workorders/crews`, {
      headers: this.headers,
    });
  }

  assignCrew(workOrderId: string, crewId: string): Observable<WorkOrder> {
    return this.http.put<WorkOrder>(
      `${this.apiBase}/api/workorders/${workOrderId}/assign-crew`,
      { crewId },
      { headers: this.headers }
    );
  }
}

import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'environments/environment';
import { ListParams, PagedResponse } from '../util/util.model';
import { RiskAssessment } from './risk.models';

@Injectable({
  providedIn: 'root',
})
export class RiskService {
  private readonly http = inject(HttpClient);
  private readonly apiBase = environment.apiBase;
  private readonly tenantId = environment.tenantId || 'acme-corp';

  private get headers() {
    return { 'X-Tenant-Id': this.tenantId };
  }

  listRiskAssessments(
    params: ListParams
  ): Observable<PagedResponse<RiskAssessment>> {
    const httpParams = new HttpParams()
      .set('pageNumber', params.pageNumber)
      .set('pageSize', params.pageSize);

    return this.http.get<PagedResponse<RiskAssessment>>(
      `${this.apiBase}/api/risk`,
      { params: httpParams, headers: this.headers }
    );
  }

  getRiskByTicketId(ticketId: string): Observable<RiskAssessment> {
    return this.http.get<RiskAssessment>(
      `${this.apiBase}/api/risk/ticket/${ticketId}`,
      { headers: this.headers }
    );
  }
}

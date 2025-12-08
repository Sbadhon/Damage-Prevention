import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable()
export class TenantInterceptor implements HttpInterceptor {
  private readonly tenantId = 'acme-corp'; // later dynamic

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const cloned = req.clone({
      setHeaders: {
        'X-Tenant-Id': this.tenantId,
      },
    });

    return next.handle(cloned);
  }
}

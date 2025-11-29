SchedulingSvc – Damage Prevention SaaS (Tenant-Aware Work Order Service)

SchedulingSvc is a multi-tenant, DDD-style microservice responsible for creating and managing work orders for dig tickets in a damage-prevention SaaS platform.

It provides:

A tenant-aware HTTP API (/api/workorders) for creating, reading, updating, and managing work orders.

Subscription to TicketSubmittedEvent (from TicketSvc) via MassTransit + RabbitMQ, automatically creating tenant-scoped work orders.

Strict tenant isolation across HTTP requests and background processing.

Tech & Patterns

.NET net9.0

ASP.NET Core Web API

MediatR for CQRS (commands/queries + pipeline behaviors)

DDD-style layers: Api, Application, Domain, Infrastructure

Multi-tenancy:

Tenant resolved from JWT claims (tenant, tid, or AAD tenant claim)

Fallback: X-Tenant-Id HTTP header

Tenant injected into commands/queries via TenantBehavior pipeline

WorkOrder aggregate stores TenantId

Messaging:

MassTransit + RabbitMQ for cross-service events

Background processor TicketSubmittedEventProcessor subscribes to TicketSubmittedEvent

Events carry TenantId to maintain tenant scope

Multi-Tenant Flow
1. HTTP Requests

Client sends request (POST, GET, etc.) to /api/workorders.

TenantResolutionMiddleware runs early:

Authenticated users → tenant from JWT claims

Fallback → X-Tenant-Id header

Stored in HttpContext.Items["__tenantId"]

HttpTenantProvider exposes CurrentTenantId via ITenantProvider.

MediatR TenantBehavior:

Sets TenantId on IRequest<T> commands if not already set

Throws if tenant cannot be resolved

Handlers:

Create aggregates (WorkOrder.CreateFromTicket) with tenant

Enforce tenant isolation on reads/writes

Persist via IWorkOrderRepository

2. Cross-Service Flow: TicketSvc → SchedulingSvc
Step 1: TicketSvc publishes event

When a ticket is submitted, SubmitTicketCommandHandler publishes tenant-aware TicketSubmittedEvent via MassTransit → RabbitMQ.

public sealed record TicketSubmittedEvent(
    string TenantId,
    Guid TicketId,
    string WorkType,
    string Address,
    double Lat,
    double Lon,
    DateTimeOffset SubmittedAt
);

Step 2: SchedulingSvc subscribes

TicketSubmittedEventProcessor (BackgroundService) subscribes to the RabbitMQ exchange for TicketSubmittedEvent.

MassTransit handles queues, routing, and deserialization automatically.

Step 3: Event processing

Processor receives TicketSubmittedEvent (including TenantId).

Creates a tenant-scoped CreateWorkOrderCommand:

var cmd = new CreateWorkOrderCommand
{
    TenantId = evt.TenantId,
    TicketId = evt.TicketId,
    WorkType = evt.WorkType,
    Address  = evt.Address,
    Lat      = evt.Lat,
    Lon      = evt.Lon
};
await _mediator.Send(cmd);


TenantBehavior respects TenantId from the event → work order is correctly tenant-scoped.

Optional Diagram
[TicketSvc] --(TicketSubmittedEvent)--> [RabbitMQ Exchange] --(MassTransit)--> [SchedulingSvc: TicketSubmittedEventProcessor] --> CreateWorkOrderCommand --> WorkOrder

Running the Service

From the repo root:

# Restore and build entire solution (optional)
dotnet restore
dotnet build

# Run SchedulingSvc only
cd src/Services/SchedulingSvc
dotnet run


Swagger available at: https://localhost:{port}/swagger (if ASPNETCORE_ENVIRONMENT=Development)

API Reference (Tenant-Aware)
1. Create Work Order

URL: POST /api/workorders

Headers:

Content-Type: application/json

Authorization: Bearer <token> (if auth enabled)

X-Tenant-Id: acme-corp (optional fallback)

Body:

{
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.0900
}


Response: 202 Accepted

{
  "workOrderId": "e4785d74-8ec0-4a8b-8d91-40f2b884c7c5"
}

2. Get Work Order By Id

URL: GET /api/workorders/{id}

Returns only if the work order belongs to the current tenant.

Sample cURL:

curl "https://localhost:5002/api/workorders/e4785d74-8ec0-4a8b-8d91-40f2b884c7c5" \
  -H "X-Tenant-Id: acme-corp"


Response (200 OK):

{
  "workOrderId": "e4785d74-8ec0-4a8b-8d91-40f2b884c7c5",
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.09,
  "crewId": null,
  "status": "Pending"
}


Response (404 Not Found): if work order does not belong to tenant.

3. List Work Orders (Paged)

URL: GET /api/workorders?pageNumber={pageNumber}&pageSize={pageSize}

Returns tenant-scoped, paged results.

Sample Response:

{
  "items": [
    {
      "workOrderId": "e4785d74-8ec0-4a8b-8d91-40f2b884c7c5",
      "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
      "workType": "Locate fiber",
      "address": "123 Main St, Saint Paul, MN",
      "lat": 44.9537,
      "lon": -93.09,
      "crewId": null,
      "status": "Pending"
    }
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 10
}

4. Assign Crew

URL: POST /api/workorders/{id}/assign?crewId={crewId}

Response: 204 No Content

5. Complete Work Order

URL: POST /api/workorders/{id}/complete

Response: 204 No Content

6. Cancel Work Order

URL: POST /api/workorders/{id}/cancel

Response: 204 No Content

Notes & Next Steps

Service mirrors TicketSvc to maintain vertical slice consistency.

Multi-tenancy is enforced both in HTTP requests and background events.

Background job/event processing respects tenant isolation via TenantId in commands.
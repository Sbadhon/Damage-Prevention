# SchedulingSvc – Damage Prevention SaaS (Tenant-Aware Work Order Service)

`SchedulingSvc` is a multi-tenant, DDD-style microservice responsible for **creating and managing work orders** for dig tickets in a damage-prevention SaaS platform.

It:

- Exposes a tenant-aware HTTP API (`/api/workorders`) for CRUD-ish operations on work orders.
- Subscribes to **`TicketSubmittedEvent`** (from `TicketSvc`) via Azure Service Bus and automatically creates work orders.
- Enforces **tenant isolation** across HTTP and background processing.

---

## Tech & Patterns

- **.NET** `net9.0`
- **ASP.NET Core** Web API (controllers)
- **MediatR** for CQRS (commands/queries + pipeline behaviors)
- **DDD-style layers**: `Api`, `Application`, `Domain`, `Infrastructure`
- **Multi-tenancy**
  - Tenant resolved primarily from **JWT claims** (`tenant`, `tid`, or AAD tenant claim)
  - Fallback to `X-Tenant-Id` header
  - Tenant injected into commands/queries via MediatR pipeline behavior
  - `WorkOrder` aggregate stores `TenantId`
- **Messaging**
  - Azure Service Bus subscriber (`TicketSubmittedEventProcessor`) for `TicketSubmittedEvent`
  - Uses `TenantId` from the event to create tenant-scoped work orders

---

## Folder Structure

At `src/Services/SchedulingSvc`:

```text
SchedulingSvc/
  Api/
    Contracts/
      WorkOrders/
        CreateWorkOrderRequest.cs     # HTTP request DTO for POST /api/workorders
        WorkOrderResponse.cs          # HTTP response DTO for work order reads
    Controllers/
      WorkOrdersController.cs         # HTTP endpoints for work orders
    Middleware/
      TenantResolutionMiddleware.cs   # Resolves tenant id from JWT claims, then header
    Tenancy/
      HttpTenantProvider.cs           # Exposes current tenant to the Application layer

  Application/
    Common/
      Behaviors/
        TenantBehavior.cs             # MediatR pipeline that injects TenantId into tenant-scoped requests
      Tenancy/
        ITenantProvider.cs            # Abstraction for current tenant
        ITenantScopedRequest.cs       # Marker interface for tenant-aware commands/queries
    WorkOrders/
      Commands/
        CreateWorkOrderCommand.cs     # Create a work order from a ticket (tenant-scoped)
        AssignCrewCommand.cs          # Assign crew to a work order (tenant-scoped)
        CompleteWorkOrderCommand.cs   # Complete a work order (tenant-scoped)
        CancelWorkOrderCommand.cs     # Cancel a work order (tenant-scoped)
      Dtos/
        WorkOrderDto.cs               # Application-layer projection of WorkOrder aggregate
      Queries/
        GetWorkOrderByIdQuery.cs      # Tenant-enforced query to fetch work order by id
        ListWorkOrdersQuery.cs        # Tenant-enforced query to page through work orders

  Domain/
    Abstractions/
      IWorkOrderRepository.cs         # Repository abstraction for WorkOrder aggregate (incl. paging by tenant)
    WorkOrders/
      WorkOrder.cs                    # WorkOrder aggregate root (TenantId + lifecycle + crew assignment)

  Infrastructure/
    WorkOrders/
      InMemoryWorkOrderRepository.cs  # In-memory repository for WorkOrders (implements paging)
    Messaging/
      TicketSubmittedEventProcessor.cs# Background service: subscribes to TicketSubmittedEvent from Service Bus

  appsettings.json
  appsettings.Development.json
  Program.cs
  SchedulingSvc.csproj
# SchedulingSvc – Damage Prevention SaaS (Tenant-Aware Work Order Service)

`SchedulingSvc` is a multi-tenant, DDD-style microservice responsible for **creating and managing work orders** for dig tickets in a damage-prevention SaaS platform.

It:

- Exposes a tenant-aware HTTP API (`/api/workorders`) for CRUD-ish operations on work orders.
- Subscribes to **`TicketSubmittedEvent`** (from `TicketSvc`) via Azure Service Bus and automatically creates work orders.
- Enforces **tenant isolation** across HTTP and background processing.

---

## Tech & Patterns

- **.NET** `net9.0`
- **ASP.NET Core** Web API (controllers)
- **MediatR** for CQRS (commands/queries + pipeline behaviors)
- **DDD-style layers**: `Api`, `Application`, `Domain`, `Infrastructure`
- **Multi-tenancy**
  - Tenant resolved primarily from **JWT claims** (`tenant`, `tid`, or AAD tenant claim)
  - Fallback to `X-Tenant-Id` header
  - Tenant injected into commands/queries via MediatR pipeline behavior
  - `WorkOrder` aggregate stores `TenantId`
- **Messaging**
  - Azure Service Bus subscriber (`TicketSubmittedEventProcessor`) for `TicketSubmittedEvent`
  - Uses `TenantId` from the event to create tenant-scoped work orders

---

## Folder Structure

At `src/Services/SchedulingSvc`:

```text
SchedulingSvc/
  Api/
    Contracts/
      WorkOrders/
        CreateWorkOrderRequest.cs     # HTTP request DTO for POST /api/workorders
        WorkOrderResponse.cs          # HTTP response DTO for work order reads
    Controllers/
      WorkOrdersController.cs         # HTTP endpoints for work orders
    Middleware/
      TenantResolutionMiddleware.cs   # Resolves tenant id from JWT claims, then header
    Tenancy/
      HttpTenantProvider.cs           # Exposes current tenant to the Application layer

  Application/
    Common/
      Behaviors/
        TenantBehavior.cs             # MediatR pipeline that injects TenantId into tenant-scoped requests
      Tenancy/
        ITenantProvider.cs            # Abstraction for current tenant
        ITenantScopedRequest.cs       # Marker interface for tenant-aware commands/queries
    WorkOrders/
      Commands/
        CreateWorkOrderCommand.cs     # Create a work order from a ticket (tenant-scoped)
        AssignCrewCommand.cs          # Assign crew to a work order (tenant-scoped)
        CompleteWorkOrderCommand.cs   # Complete a work order (tenant-scoped)
        CancelWorkOrderCommand.cs     # Cancel a work order (tenant-scoped)
      Dtos/
        WorkOrderDto.cs               # Application-layer projection of WorkOrder aggregate
      Queries/
        GetWorkOrderByIdQuery.cs      # Tenant-enforced query to fetch work order by id
        ListWorkOrdersQuery.cs        # Tenant-enforced query to page through work orders

  Domain/
    Abstractions/
      IWorkOrderRepository.cs         # Repository abstraction for WorkOrder aggregate (incl. paging by tenant)
    WorkOrders/
      WorkOrder.cs                    # WorkOrder aggregate root (TenantId + lifecycle + crew assignment)

  Infrastructure/
    WorkOrders/
      InMemoryWorkOrderRepository.cs  # In-memory repository for WorkOrders (implements paging)
    Messaging/
      TicketSubmittedEventProcessor.cs# Background service: subscribes to TicketSubmittedEvent from Service Bus

  appsettings.json
  appsettings.Development.json
  Program.cs
  SchedulingSvc.csproj
How It Works (High-Level Flow)
1. Multi-tenant request handling (HTTP)
Client calls something like POST /api/workorders or GET /api/workorders.
TenantResolutionMiddleware runs early:
If user is authenticated, resolves tenant from JWT claims:
tenant, tid, or http://schemas.microsoft.com/identity/claims/tenantid
If not found in claims, falls back to the X-Tenant-Id header.
Stores tenant id in HttpContext.Items["__tenantId"].
HttpTenantProvider:
Reads tenant from HttpContext.Items.
Exposes it via ITenantProvider.CurrentTenantId.
WorkOrdersController:
Maps HTTP requests to MediatR commands/queries (CreateWorkOrderCommand, ListWorkOrdersQuery, etc.).
Sends them via ISender.
TenantBehavior (MediatR pipeline):
Runs for any IRequest<T> that implements ITenantScopedRequest.
If TenantId is already set on the request → does nothing (used by background jobs).
Otherwise, reads tenant from ITenantProvider and sets request.TenantId.
Throws if no tenant can be resolved.
Handlers:
Use tenant to:
Create tenant-scoped aggregates (WorkOrder.CreateFromTicket(tenantId, ...)).
Enforce isolation on reads (workOrder.TenantId == request.TenantId).
Persist via IWorkOrderRepository.
2. Cross-service flow: TicketSvc → SchedulingSvc
In TicketSvc, when a ticket is submitted, SubmitTicketCommandHandler publishes a tenant-aware TicketSubmittedEvent to Azure Service Bus:
public sealed record TicketSubmittedEvent(
    string TenantId,
    Guid TicketId,
    string WorkType,
    string Address,
    double Lat,
    double Lon,
    DateTimeOffset SubmittedAt
);
In SchedulingSvc, TicketSubmittedEventProcessor is a BackgroundService that:
Connects to Service Bus topic (e.g. ticket-submitted).
Deserializes TicketSubmittedEvent (including TenantId).
Creates a CreateWorkOrderCommand with TenantId set from the event:
var cmd = new CreateWorkOrderCommand
{
    TenantId = evt.TenantId,
    TicketId = evt.TicketId,
    WorkType = evt.WorkType,
    Address  = evt.Address,
    Lat      = evt.Lat,
    Lon      = evt.Lon
};
Sends it via MediatR (ISender).
TenantBehavior in SchedulingSvc:
Sees TenantId is already set on the command (from the event).
Does not override it and does not require HttpContext.
The resulting WorkOrder is created with the correct tenant and can later be queried only by that tenant.
This makes the entire path:
Tenant in JWT/header → TicketSvc → TicketSubmittedEvent(TenantId, …) → SchedulingSvc → WorkOrder
fully multi-tenant and consistent.
Running SchedulingSvc Locally
From the repo root:
# (optional) Restore/build entire solution
dotnet restore
dotnet build

# Run SchedulingSvc only
cd src/Services/SchedulingSvc
dotnet run
If ASPNETCORE_ENVIRONMENT=Development, Swagger will be available at:
https://localhost:{port}/swagger
To exercise the background processor, you’ll need:
Azure Service Bus connection string in ConnectionStrings:ServiceBus
Matching topic & subscription names for TicketSubmittedEventProcessor.
API Endpoints
All endpoints are tenant-aware:
For authenticated scenarios, tenant is resolved from JWT claims.
For simpler dev/testing, you can send X-Tenant-Id: acme-corp directly.
1. Create Work Order
Usually used for manual creation (the event processor also uses the same command under the hood).
URL
POST /api/workorders
Headers
Content-Type: application/json
Authorization: Bearer <token> (if using auth)
X-Tenant-Id: acme-corp (optional fallback for dev)
Body – CreateWorkOrderRequest
{
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.0900
}
Sample cURL
curl -X POST "https://localhost:5002/api/workorders" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: acme-corp" \
  -d '{
    "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
    "workType": "Locate fiber",
    "address": "123 Main St, Saint Paul, MN",
    "lat": 44.9537,
    "lon": -93.0900
  }'
Response – 202 Accepted
{
  "workOrderId": "e4785d74-8ec0-4a8b-8d91-40f2b884c7c5"
}
2. Get Work Order By Id
URL
GET /api/workorders/{id}
Behavior
Returns the work order only if it belongs to the current tenant.
Sample cURL
curl "https://localhost:5002/api/workorders/e4785d74-8ec0-4a8b-8d91-40f2b884c7c5" \
  -H "X-Tenant-Id: acme-corp"
Response – 200 OK
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
If not found or belongs to another tenant:
404 Not Found
3. List Work Orders (Per Tenant, Paged)
URL
GET /api/workorders?pageNumber={pageNumber}&pageSize={pageSize}
Query Params
pageNumber (default 1)
pageSize (default 20)
Sample cURL
curl "https://localhost:5002/api/workorders?pageNumber=1&pageSize=10" \
  -H "X-Tenant-Id: acme-corp"
Response – 200 OK
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
    // ...
  ],
  "totalCount": 10,
  "pageNumber": 1,
  "pageSize": 10
}
4. Assign Crew
URL
POST /api/workorders/{id}/assign?crewId={crewId}
Sample cURL
curl -X POST "https://localhost:5002/api/workorders/e4785d74-8ec0-4a8b-8d91-40f2b884c7c5/assign?crewId=crew-123" \
  -H "X-Tenant-Id: acme-corp"
Response
204 No Content
5. Complete Work Order
URL
POST /api/workorders/{id}/complete
Sample cURL
curl -X POST "https://localhost:5002/api/workorders/e4785d74-8ec0-4a8b-8d91-40f2b884c7c5/complete" \
  -H "X-Tenant-Id: acme-corp"
Response
204 No Content
6. Cancel Work Order
URL
POST /api/workorders/{id}/cancel
Sample cURL
curl -X POST "https://localhost:5002/api/workorders/e4785d74-8ec0-4a8b-8d91-40f2b884c7c5/cancel" \
  -H "X-Tenant-Id: acme-corp"
Response
204 No Content
Notes & Next Steps


This service is intentionally shaped to mirror TicketSvc so we can reason about the whole platform as vertical slices with consistent SaaS + DDD patterns.

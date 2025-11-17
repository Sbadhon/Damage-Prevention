# TicketSvc – Damage Prevention SaaS (Tenant-Aware Ticket Service)

`TicketSvc` is a multi-tenant, DDD-style microservice responsible for **creating and managing dig tickets** in a damage-prevention SaaS platform.

It exposes an HTTP API (`/api/tickets`) and publishes **`TicketSubmittedEvent`** messages to a configured message bus (Kafka or Azure Service Bus) so downstream services (Scheduling, Risk, etc.) can react.

---

## Tech & Patterns

- **.NET** `net9.0`
- **ASP.NET Core** Web API (controllers)
- **MediatR** for CQRS (commands/queries + pipeline behaviors)
- **DDD-style layers**: `Api`, `Application`, `Domain`, `Infrastructure`
- **Multi-tenancy**
  - Tenant resolved **primarily from JWT claims** (`tenant`, `tid`, or AAD tenant claim)
  - Fallback to `X-Tenant-Id` header (useful for dev / non-auth flows)
  - Tenant injected into commands/queries via MediatR pipeline behavior
  - `Ticket` aggregate stores `TenantId`
- **Messaging**
  - Kafka publisher (`KafkaTicketEventPublisher`)
  - Azure Service Bus publisher (`AzureServiceBusTicketEventPublisher`)
  - Switchable via configuration, with `NoOpTicketEventPublisher` fallback

---

## Folder Structure

At `src/Services/TicketSvc`:

```text
TicketSvc/
  Api/
    Contracts/
      Tickets/
        SubmitTicketRequest.cs      # HTTP request DTO for POST /api/tickets
        TicketResponse.cs           # HTTP response DTO for ticket reads
    Controllers/
      TicketsController.cs          # HTTP endpoints for tickets
    Middleware/
      TenantResolutionMiddleware.cs # Resolves tenant id from JWT claims, then header
    Tenancy/
      HttpTenantProvider.cs         # Exposes current tenant to the Application layer

  Application/
    Common/
      Behaviors/
        TenantBehavior.cs           # MediatR pipeline that injects TenantId into tenant-scoped requests
      Tenancy/
        ITenantProvider.cs          # Abstraction for current tenant
        ITenantScopedRequest.cs     # Marker interface for tenant-aware commands/queries
    Tickets/
      Commands/
        SubmitTicketCommand.cs      # Command to submit a ticket (tenant-scoped)
        CompleteTicketCommand.cs    # Command to complete a ticket (tenant-scoped)
        CancelTicketCommand.cs      # Command to cancel a ticket (tenant-scoped)
      Dtos/
        TicketDto.cs                # Application-layer projection of Ticket aggregate
      Queries/
        GetTicketByIdQuery.cs       # Tenant-enforced query to fetch ticket by id
        ListTicketsQuery.cs         # Tenant-enforced query to page through tickets

  Domain/
    Abstractions/
      ITicketRepository.cs          # Repository abstraction for Ticket aggregate (incl. paging by tenant)
    Events/
      ITicketEventPublisher.cs      # Abstraction for publishing ticket events
    Tickets/
      Ticket.cs                     # Ticket aggregate root (TenantId + lifecycle + status)

  Infrastructure/
    Events/
      AzureServiceBusTicketEventPublisher.cs # Publishes TicketSubmittedEvent to Azure Service Bus
      KafkaTicketEventPublisher.cs           # Publishes TicketSubmittedEvent to Kafka
      NoOpTicketEventPublisher.cs            # No-op implementation for local/dev
    Tickets/
      InMemoryTicketRepository.cs            # In-memory repository for Tickets (implements paging)

  appsettings.json
  appsettings.Development.json
  Program.cs
  TicketSvc.csproj
How It Works (High-Level Flow)
1. Multi-tenant request handling
Client calls POST /api/tickets (or any other endpoint).
TenantResolutionMiddleware runs early in the pipeline:
If the user is authenticated, it tries to resolve tenant from JWT claims:
tenant, tid, or http://schemas.microsoft.com/identity/claims/tenantid
If not found in claims, it falls back to the X-Tenant-Id header.
Stores the resolved tenant id in HttpContext.Items["__tenantId"].
HttpTenantProvider:
Reads the tenant from HttpContext.Items.
Exposes it via ITenantProvider.CurrentTenantId to the Application layer.
TicketsController:
Receives the HTTP request and maps it to a MediatR command/query (e.g., SubmitTicketCommand).
Sends it via ISender (MediatR).
TenantBehavior (MediatR pipeline behavior):
Runs before the handler on any request implementing ITenantScopedRequest.
Reads the tenant from ITenantProvider.
Sets request.TenantId.
Throws if no tenant is available.
Handlers (Commands/Queries):
Use TenantId to:
Create tenant-scoped aggregates (Ticket.CreateDraft(tenantId, ...)).
Enforce isolation when reading (ticket.TenantId == request.TenantId).
Persist via ITicketRepository.
For SubmitTicketCommand, publish TicketSubmittedEvent through ITicketEventPublisher.
2. Messaging
ITicketEventPublisher is resolved based on configuration in Program.cs:
Azure Service Bus
TicketEvents:UseAzureServiceBus = true
Uses AzureServiceBusTicketEventPublisher.
Kafka
TicketEvents:UseKafka = true (and UseAzureServiceBus = false)
Uses KafkaTicketEventPublisher.
Fallback (NoOp)
If neither flag is true, defaults to NoOpTicketEventPublisher (no events published – useful for local dev/unit tests).
Running TicketSvc Locally
From the repo root:
# (optional) Restore/build entire solution
dotnet restore
dotnet build

# Run TicketSvc only
cd src/Services/TicketSvc
dotnet run
The service will start on the usual ASP.NET Core ports (check console output), and if Environment is Development, Swagger will be available at:
https://localhost:{port}/swagger
API Endpoints
1. Submit Ticket
URL
POST /api/tickets
Headers
Content-Type: application/json
Auth: standard Bearer JWT (if using auth)
X-Tenant-Id: acme-corp (optional fallback when JWT doesn’t include tenant)
Request Body – SubmitTicketRequest
{
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.0900
}
Sample cURL
curl -X POST "https://localhost:5001/api/tickets" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: acme-corp" \
  -d '{
    "workType": "Locate fiber",
    "address": "123 Main St, Saint Paul, MN",
    "lat": 44.9537,
    "lon": -93.0900
  }'
Response – 202 Accepted
{
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "status": "Submitted"
}
2. Get Ticket By Id
URL
GET /api/tickets/{id}
Behavior
Returns the ticket only if it belongs to the current tenant (enforced in GetTicketByIdQueryHandler).
Sample cURL
curl "https://localhost:5001/api/tickets/6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e" \
  -H "X-Tenant-Id: acme-corp"
Response – 200 OK (example)
{
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.09,
  "status": "Submitted"
}
If the ticket does not exist or belongs to another tenant:
404 Not Found
3. List Tickets (Per Tenant, Paged)
URL
GET /api/tickets?pageNumber={pageNumber}&pageSize={pageSize}
Query Parameters
pageNumber (optional, default 1)
pageSize (optional, default 20)
Sample cURL
curl "https://localhost:5001/api/tickets?pageNumber=1&pageSize=10" \
  -H "X-Tenant-Id: acme-corp"
Response – 200 OK
{
  "items": [
    {
      "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
      "workType": "Locate fiber",
      "address": "123 Main St, Saint Paul, MN",
      "lat": 44.9537,
      "lon": -93.09,
      "status": "Submitted"
    }
    // ...
  ],
  "totalCount": 42,
  "pageNumber": 1,
  "pageSize": 10
}
Only tickets for the current tenant are returned.
4. Complete Ticket
URL
POST /api/tickets/{id}/complete
Marks a ticket as Completed (if it belongs to the current tenant and is in a valid state).
Sample cURL
curl -X POST "https://localhost:5001/api/tickets/6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e/complete" \
  -H "X-Tenant-Id: acme-corp"
Response
204 No Content
5. Cancel Ticket
URL
POST /api/tickets/{id}/cancel
Body (optional plain string, e.g. reason)
"Customer requested cancellation"
Sample cURL
curl -X POST "https://localhost:5001/api/tickets/6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e/cancel" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: acme-corp" \
  -d '"Customer requested cancellation"'
Response
204 No Content
If the ticket is not found or belongs to a different tenant, the command handlers throw; you can adapt your global exception handling to return appropriate HTTP codes (404 / 403).
Notes
This TicketSvc is designed as one vertical slice in a broader SaaS system.
Other services (Scheduling, Risk, AssetCatalog, etc.) can subscribe to TicketSubmittedEvent and implement their own DDD + tenant-aware logic in the same style.
For real production usage, replace InMemoryTicketRepository with an EF Core implementation that:
Filters by TenantId at query time, and
Stores TenantId as a column in your tickets table.

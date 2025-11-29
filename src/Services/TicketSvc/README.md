# TicketSvc — Tenant-Aware Ticket Service

**TicketSvc** is a multi-tenant microservice responsible for creating, submitting, and managing dig tickets.
It uses DDD, CQRS, and event-driven messaging to publish tenant-scoped TicketSubmittedEvent messages to downstream services such as SchedulingSvc and RiskSvc.

## Tech & Architecture

- **.NET 9.0**  
- **ASP.NET Core Web API + Controllers**  
- **MediatR (v11)** – CQRS Commands, Queries, Pipeline Behaviors  
- **DDD Layers**: `Api → Application → Domain → Infrastructure`
- **Multi-Tenancy**  
  - Tenant resolved from JWT claims first  
  - Fallback to `X-Tenant-Id` header  
  - Tenant injected into commands via `TenantBehavior`  
- **MassTransit + RabbitMQ**  
  - `TicketSubmittedConsumer` listens for `TicketSubmittedEvent`  
  - Creates tenant-scoped risk assessments 
- **Shared Building Blocks**  
  - `SharedKernel` (IDateTime, AggregateRoot, etc.)  
  - `Contracts` (shared integration events)

## High-Level Flow
1. Multi-Tenant HTTP Request Handling
- TenantResolutionMiddleware extracts the tenant from:

- **JWT claim:**
tenant, tid, or http://schemas.microsoft.com/identity/claims/tenantid Or header: X-Tenant-Id: acme-corp
- The resolved tenant is stored in HttpContext.Items.
- TenantBehavior (MediatR pipeline):
- Automatically applies the tenant to any ITenantScopedRequest
- Rejects the request if no tenant can be determined
- Does not override tenant when provided manually (e.g., in event-driven flows)

2. Domain Behavior (Ticket Aggregate)
- **A Ticket:**
- is scoped to a tenant
- manages lifecycle transitions:

Created → Submitted → Completed / Cancelled

- emits domain events when transitioning
- The most important domain event is:

- **TicketSubmittedEvent**
which includes TenantId, making it safe for cross-service consumption.

- **Event-Driven Mode (Primary Integration Path)**
- When a ticket is submitted:
- Ticket moves to Submitted state
- A TicketSubmittedEvent is produced
- MassTransit publishes it to RabbitMQ
- Downstream services receive tenant-aware event data:
- SchedulingSvc → automatically creates a work order
- RiskSvc → automatically performs risk assessment

## API Endpoints
**All endpoints require tenant context**:

- JWT (preferred)  
- Header: `X-Tenant-Id: acme-corp`

### 1. Submit a Ticket
POST /api/tickets
**Response Example:**
```json
{
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.09
}
```
Response (202 Accepted)

### 2.Get Ticket by Id
GET /api/tickets/{id}
**Response Example:**
 - Returns the ticket only if it belongs to the current tenant
 - Otherwise → 404 Not Found

### 3. List Tickets
GET /api/tickets?pageNumber=1&pageSize=20
 - Paged and tenant-scoped results.

### 4. Complete Ticket
POST /api/tickets/{id}/complete

### 5. Cancel Ticket
POST /api/tickets/{id}/cancel

## Running Locally
```bash
cd src/Services/TicketSvc
dotnet run
```
## Swagger UI (Development):
https://localhost:{port}/swagger

## Notes 
 - Multi-tenancy is enforced at:
 - Request middleware
 - MediatR pipeline
 - Domain layer
 - Message publishing
 - Event-driven downstream services rely on TenantId for isolation.
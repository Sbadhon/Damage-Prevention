TicketSvc — Tenant-Aware Ticket Service
TicketSvc is a lightweight, multi-tenant microservice responsible for creating, submitting, and managing dig tickets. It follows clean DDD/CQRS patterns and publishes a TicketSubmittedEvent for downstream services.
Overview
.NET 9 + ASP.NET Core
CQRS with MediatR
DDD Layers: API → Application → Domain → Infrastructure
Multi-Tenancy: Tenant resolved from JWT or X-Tenant-Id
Messaging: Kafka / Azure Service Bus / No-Op fallback
Aggregate: Ticket (tenant-scoped, lifecycle-driven)
Architecture (High-Level)
flowchart LR
    A[API Layer<br/>Controllers] --> B[MediatR<br/>Commands & Queries]
    B --> C[Application Layer<br/>TenantBehavior]
    C --> D[Domain Layer<br/>Ticket Aggregate]
    D --> E[Infrastructure<br/>TicketRepository]
    D --> F[Messaging<br/>TicketEventPublisher]
Folder Structure
TicketSvc/
  Api/                # Controllers, middleware, request models
  Application/        # Commands, queries, DTOs, behaviors
  Domain/             # Aggregates, domain events, repository abstractions
  Infrastructure/     # Repositories + message publishers
  Program.cs
  appsettings*.json
Key Concepts
Multi-Tenant Enforcement
Tenant is resolved in middleware and applied to every request implementing:
ITenantScopedRequest
Only tickets belonging to the current tenant are returned or mutated.
Ticket Submitted Event
When a ticket is submitted:
Ticket -> TicketSubmittedEvent -> Kafka / ServiceBus / NoOp
Downstream services (Risk, Scheduling, etc.) can subscribe.
Event example:
{
  "ticketId": "uuid",
  "tenantId": "acme-corp",
  "workType": "Locate fiber",
  "address": "123 Main St",
  "submittedAt": "2025-01-01T12:00:00Z"
}
Running Locally
cd src/Services/TicketSvc
dotnet run
Swagger (Development):
https://localhost:{port}/swagger
API Examples
Submit Ticket
POST /api/tickets
{
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.09
}
Get Ticket
GET /api/tickets/{id}
List Tickets
GET /api/tickets?pageNumber=1&pageSize=20
Complete Ticket
POST /api/tickets/{id}/complete
Cancel Ticket
POST /api/tickets/{id}/cancel
Notes
Replace InMemoryTicketRepository with EF Core in production.
TenantId is a value object enforced in the domain.
Messaging transport activated via appsettings flags.
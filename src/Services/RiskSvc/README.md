RiskSvc – Damage Prevention SaaS (Tenant-Aware Risk Assessment Service)
RiskSvc is a multi-tenant, domain-driven microservice responsible for computing and storing risk assessments for dig tickets.
It operates in two modes:
Event-Driven Mode (Primary)
Subscribes to TicketSubmittedEvent from TicketSvc
Automatically performs a risk assessment
Stores results tenant-isolated
Query Mode
Allows UI and other services to fetch risk by ticket or list risk assessments by tenant
RiskSvc follows the same architecture as TicketSvc and SchedulingSvc:
Tenant-aware
CQRS + MediatR
DDD aggregates
Azure Service Bus subscriber
In-memory repository (pluggable with EFCore later)
Tech & Architecture
✔ .NET net9.0
✔ ASP.NET Core Web API + Controllers
✔ MediatR (v11) – CQRS Commands, Queries, Pipeline Behaviors
✔ DDD Layers: Api → Application → Domain → Infrastructure
✔ Multi-Tenancy
Tenant resolved from JWT claims first
Fallback to X-Tenant-Id header
Tenant injected into commands via TenantBehavior
RiskAssessment aggregate stores TenantId
✔ Azure Service Bus
Background TicketRiskAssessmentProcessor listens to TicketSubmittedEvent
Creates tenant-scoped risk assessments
✔ Shared Building Blocks
SharedKernel (IDateTime, AggregateRoot, etc.)
Contracts (shared integration events)
Folder Structure
RiskSvc/
  Api/
    Contracts/
      Risk/
        RiskAssessmentResponse.cs
    Controllers/
      RiskController.cs
    Middleware/
      TenantResolutionMiddleware.cs
    Tenancy/
      HttpTenantProvider.cs

  Application/
    Common/
      Behaviors/
        TenantBehavior.cs
      Tenancy/
        ITenantProvider.cs
        ITenantScopedRequest.cs
    Risk/
      Commands/
        RecordRiskAssessmentCommand.cs
      Dtos/
        RiskAssessmentDto.cs
      Queries/
        GetRiskByTicketIdQuery.cs
        ListRiskAssessmentsQuery.cs

  Domain/
    Abstractions/
      IRiskAssessmentRepository.cs
    Risk/
      RiskAssessment.cs

  Infrastructure/
    Risk/
      InMemoryRiskAssessmentRepository.cs
    Messaging/
      TicketRiskAssessmentProcessor.cs

  appsettings.json
  appsettings.Development.json
  Program.cs
  RiskSvc.csproj
This structure is identical to TicketSvc and SchedulingSvc to enforce consistency across the platform.
High-Level Flow
1. When a Ticket Is Submitted (Cross-Service Flow)
TicketSvc → TicketSubmittedEvent (TenantId included)
                     ↓
            Azure Service Bus Topic
                     ↓
RiskSvc → TicketRiskAssessmentProcessor
                     ↓
      RecordRiskAssessmentCommand (TenantId respected)
                     ↓
      RiskAssessment aggregate created and stored
Why this matters
Risk is automatically evaluated as soon as a ticket is created, with the same tenant that originated the ticket.
This keeps all downstream processing multi-tenant consistent.
2. Tenant Resolution (HTTP Requests)
RiskSvc supports UI & API queries to fetch risk data.
Flow:
TenantResolutionMiddleware
Extracts tenant from:
JWT claims tenant, tid, AAD tenant claim
or header X-Tenant-Id
HttpTenantProvider
TenantBehavior
Injects TenantId into MediatR commands/queries
Does NOT override TenantId if it was set by the event processor (important for background events)
Queries return tenant-filtered data only
3. Risk Assessment Logic
Currently:
Simple scoring based on work type
Example:
“blast”, “explosive” → 0.9 → High
“drill”, “boring” → 0.7 → Medium
“fiber” → 0.5 → Medium
default → Low
Later, can extend with:
Location heuristics
Past incident history
Soil type / depth models
GIS overlays
Machine learning risk model
API Endpoints
All endpoints require tenant context:
JWT (preferred)
or header: X-Tenant-Id: acme-corp
1. Get Risk by Ticket Id
GET /api/risk/ticket/{ticketId}
Response Example
{
  "riskId": "e6b25c0d-77fa-4e9c-b197-f54c92bb1dec",
  "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
  "score": 0.7,
  "level": "Medium",
  "workType": "Locate fiber",
  "address": "123 Main St, Saint Paul, MN",
  "lat": 44.9537,
  "lon": -93.09,
  "assessedAt": "2025-11-14T02:54:18Z"
}
If tenant mismatch:
404 Not Found
2. List Risk Assessments (Per Tenant)
GET /api/risk?pageNumber=1&pageSize=20
Response Example
{
  "items": [
    {
      "riskId": "e6b25c0d-77fa-4e9c-b197-f54c92bb1dec",
      "ticketId": "6d2c55a5-bf13-4f1e-a9e0-4e0a0e4b4f6e",
      "score": 0.7,
      "level": "Medium",
      "workType": "Locate fiber",
      "address": "123 Main St, Saint Paul, MN",
      "lat": 44.9537,
      "lon": -93.09,
      "assessedAt": "2025-11-14T02:54:18Z"
    }
  ],
  "totalCount": 16,
  "pageNumber": 1,
  "pageSize": 20
}
Background Processing (Azure Service Bus)
TicketRiskAssessmentProcessor listens on:
Topic: ticket-submitted
Subscription: risk-svc
When a message arrives:
Deserialize TicketSubmittedEvent
Create RecordRiskAssessmentCommand
Set TenantId from the event
Save RiskAssessment
Why we do this
TenantId flows through the event, enabling:
Correct multi-tenant boundaries
Isolated data domains
Independent scaling
Event-driven decoupling
Running RiskSvc Locally
cd src/Services/RiskSvc
dotnet run
Then open:
https://localhost:{port}/swagger
To test background processing:
Provide Azure Service Bus connection in appsettings.Development.json
Make sure TicketSvc publishes tenant-aware TicketSubmittedEvent
Extending RiskSvc (What Comes Next)
Next expand RiskSvc:
✔ Add advanced ML-based risk scoring
✔ Add GIS integration (soil depth, pipe density, utility overlaps)
✔ Add alerts (“High-risk ticket submitted in your region”)
✔ Add risk rules configuration per tenant
✔ Add persistence via EF Core + PostgreSQL/CosmosDB

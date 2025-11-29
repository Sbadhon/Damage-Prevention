# RiskSvc – Damage Prevention SaaS (Tenant-Aware Risk Assessment Service)

**RiskSvc** is a multi-tenant, domain-driven microservice responsible for computing and storing risk assessments for dig tickets. It operates in two modes:

- **Event-Driven Mode (Primary)**  
  - Subscribes to `TicketSubmittedEvent` from `TicketSvc` via **MassTransit + RabbitMQ**  
  - Automatically performs a risk assessment  
  - Stores results tenant-isolated  

- **Query Mode**  
  - Allows UI and other services to fetch risk by ticket or list risk assessments by tenant  

RiskSvc follows the same architecture as `TicketSvc` and `SchedulingSvc`:

- Tenant-aware  
- CQRS + MediatR  
- DDD aggregates  
- RabbitMQ subscriber via MassTransit  
- In-memory repository (pluggable with EF Core later)

---

## Tech & Architecture

- **.NET 9.0**  
- **ASP.NET Core Web API + Controllers**  
- **MediatR (v11)** – CQRS Commands, Queries, Pipeline Behaviors  
- **DDD Layers**: `Api → Application → Domain → Infrastructure`  
- **Multi-Tenancy**  
  - Tenant resolved from JWT claims first  
  - Fallback to `X-Tenant-Id` header  
  - Tenant injected into commands via `TenantBehavior`  
  - `RiskAssessment` aggregate stores `TenantId`  
- **MassTransit + RabbitMQ**  
  - `TicketSubmittedConsumer` listens for `TicketSubmittedEvent`  
  - Creates tenant-scoped risk assessments  
- **Shared Building Blocks**  
  - `SharedKernel` (IDateTime, AggregateRoot, etc.)  
  - `Contracts` (shared integration events)

---

> This structure mirrors `TicketSvc` and `SchedulingSvc` to enforce consistency across the platform.

---

## High-Level Flow

### 1. When a Ticket Is Submitted (Cross-Service Flow)

TicketSvc → TicketSubmittedEvent (TenantId included)
↓
RabbitMQ
↓
RiskSvc → TicketSubmittedConsumer
↓
RecordRiskAssessmentCommand (TenantId respected)
↓
RiskAssessment aggregate created and stored


Risk is automatically evaluated as soon as a ticket is created, using the same tenant that originated the ticket, keeping all downstream processing tenant-consistent.

### Tenant Resolution (HTTP Requests)

**Flow for UI & API queries:**

1. `TenantResolutionMiddleware`  
   - Extracts tenant from:
     - JWT claims (`tenant`, `tid`)  
     - Header `X-Tenant-Id`
2. `HttpTenantProvider`  
3. `TenantBehavior`  
   - Injects `TenantId` into MediatR commands/queries  
   - Does NOT override TenantId set by background event processor  
4. Queries return tenant-filtered data only  

### 3. Risk Assessment Logic

Currently:

- Simple scoring based on work type:
  - `"blast"`, `"explosive"` → 0.9 → High  
  - `"drill"`, `"boring"` → 0.7 → Medium  
  - `"fiber"` → 0.5 → Medium  
  - Default → Low  

Future enhancements:

- Location heuristics  
- Past incident history  
- Soil type / depth models  
- GIS overlays  
- Machine learning risk model  

---

## API Endpoints

**All endpoints require tenant context**:

- JWT (preferred)  
- Header: `X-Tenant-Id: acme-corp`

### 1. Get Risk by Ticket Id
GET /api/risk/ticket/{ticketId}
**Response Example:**
```json
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
```
Tenant mismatch → 404 Not Found

### 2. List Risk Assessments (Per Tenant)
GET /api/risk?pageNumber=1&pageSize=20
```json
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
```
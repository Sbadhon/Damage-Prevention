Damage Prevention SaaS – Multi-Tenant DDD Microservice Suite
A small, production-style damage prevention platform inspired by 811 / KorTerra workflows:
TicketSvc – multi-tenant dig ticket intake & publishing
SchedulingSvc – work order scheduling per tenant
RiskSvc – risk scoring & analytics (with optional GIS hook)
RasterProcessingSvc – GIS/raster analysis helper
Gateway – API gateway façade (optional)
Web Client – React + Redux dashboard for tenants
Architecture is SaaS + multi-tenant, DDD-styled, CQRS + MediatR, and event-driven via Kafka or Azure Service Bus, all wired with DI and options-based config.

System Architecture (Mermaid)

flowchart LR
    subgraph FE[Frontend]
        W[React + Redux<br/>web-client]
    end

    subgraph API[Gateway / Services]
        G[Gateway<br/>(optional façade)]
        T[TicketSvc<br/>Ticket API + events]
        S[SchedulingSvc<br/>Work Orders]
        R[RiskSvc<br/>Risk Assessments]
        RP[RasterProcessingSvc<br/>GIS/Raster API]
    end

    subgraph Infra[Infrastructure]
        K[(Kafka)]
        SB[(Azure Service Bus)]
        PG[(Postgres<br/>assets/raster)]
        SQL[(SQL Server<br/>tickets/workorders if used)]
    end

    Ten[Multi-tenant Clients<br/>X-Tenant-Id/JWT] -->|HTTP<br/>(JSON)| W

    W -->|/api/...| G
    W -->|direct (dev)| T
    W -->|direct (dev)| S
    W -->|direct (dev)| R

    G -->|/api/tickets| T
    G -->|/api/workorders| S
    G -->|/api/risk| R

    T -->|TicketSubmittedEvent| K
    T -->|TicketSubmittedEvent| SB

    K --> S
    K --> R

    SB --> S
    SB --> R

    R -->|HTTP| RP

    RP --> PG
    S --> SQL
    T --> SQL

flowchart LR
    subgraph FE[Frontend]
        W[React + Redux<br/>web-client]
    end

    subgraph API[Gateway / Services]
        G[Gateway<br/>(optional façade)]
        T[TicketSvc<br/>Ticket API + events]
        S[SchedulingSvc<br/>Work Orders]
        R[RiskSvc<br/>Risk Assessments]
        RP[RasterProcessingSvc<br/>GIS/Raster API]
    end

    subgraph Infra[Infrastructure]
        K[(Kafka)]
        SB[(Azure Service Bus)]
        PG[(Postgres<br/>assets/raster)]
        SQL[(SQL Server<br/>tickets/workorders if used)]
    end

    Ten[Multi-tenant Clients<br/>X-Tenant-Id/JWT] -->|HTTP<br/>(JSON)| W

    W -->|/api/...| G
    W -->|direct (dev)| T
    W -->|direct (dev)| S
    W -->|direct (dev)| R

    G -->|/api/tickets| T
    G -->|/api/workorders| S
    G -->|/api/risk| R

    T -->|TicketSubmittedEvent| K
    T -->|TicketSubmittedEvent| SB

    K --> S
    K --> R

    SB --> S
    SB --> R

    R -->|HTTP| RP

    RP --> PG
    S --> SQL
    T --> SQL

Projects & Folder Layout
src/
  BuildingBlocks/
    SharedKernel/              # cross-cutting: IDateTime, options, etc.
    Contracts/                 # cross-service DTOs/events (TicketSubmittedEvent, etc.)

  Services/
    TicketSvc/                 # ticket intake, DDD-style, publishes TicketSubmittedEvent
    SchedulingSvc/             # work order scheduling from ticket events
    RiskSvc/                   # risk scoring and queries
    RasterProcessingSvc/       # GIS/raster helper API (Postgres-backed)

  Gateway/                     # optional API gateway façade (if enabled)

web-client/                    # React + TS + Redux tenant dashboard
docker-compose.yml             # Kafka, Postgres, SQL Server, Adminer, etc.

TicketSvc/
  Api/             # Controllers, middleware, tenancy
  Application/     # Commands, queries, DTOs, behaviors (MediatR)
  Domain/          # Aggregates, repositories, domain events
  Infrastructure/  # Impl of repositories, event publishers, GIS, etc.
  Program.cs       # Composition root
  appsettings*.json
TicketSvc/
  Api/             # Controllers, middleware, tenancy
  Application/     # Commands, queries, DTOs, behaviors (MediatR)
  Domain/          # Aggregates, repositories, domain events
  Infrastructure/  # Impl of repositories, event publishers, GIS, etc.
  Program.cs       # Composition root
  appsettings*.json

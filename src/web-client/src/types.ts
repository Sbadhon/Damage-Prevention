
export enum TicketStatus {
    Open = 'Open',
    InProgress = 'In Progress',
    Completed = 'Completed',
    Cancelled = 'Cancelled',
  }
  
  export enum WorkOrderStatus {
    Pending = 'Pending',
    Assigned = 'Assigned',
    Completed = 'Completed',
    Cancelled = 'Cancelled',
  }
  
  export enum RiskLevel {
    Low = 'Low',
    Medium = 'Medium',
    High = 'High',
    Critical = 'Critical',
  }
  
  export interface Ticket {
    ticketId: string;
    description: string;
    status: TicketStatus;
    crewId?: string;
    workType: string;
    address: string;
    createdAt: string;
    lat: number;
    lon: number;
  }
  
  export interface WorkOrder {
    workOrderId: string;
    ticketId: string;
    crewId?: string;
    crewName?: string;
    status: WorkOrderStatus;
    details: string;
    scheduledAt: string;
    createdAt: string;
  }
  
  export interface RiskAssessment {
    riskId: string;
    ticketId: string;
    score: number;
    level: RiskLevel;
    workType: string;
    address: string;
    lat: number;
    lon: number;
    assessedAt: string;
  }

  export interface Crew {
    crewId: string;
    crewName: string;
    specialty: string;
  }
  
  export interface PagedResponse<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }
  
  export interface DashboardStats {
      tickets: {
          open: number;
          inProgress: number;
          completed: number;
      };
      workOrders: {
          pending: number;
          assigned: number;
          completed: number;
      };
      risks: {
          high: number;
          critical: number;
      };
  }
  
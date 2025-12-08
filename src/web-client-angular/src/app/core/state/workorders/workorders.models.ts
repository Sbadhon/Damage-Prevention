export enum WorkOrderStatus {
  Pending = 'Pending',
  Assigned = 'Assigned',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
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

export interface Crew {
  crewId: string;
  crewName: string;
  specialty: string;
}

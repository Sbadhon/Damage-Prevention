
export enum TicketStatus {
  Open = 'Open',
  InProgress = 'In Progress',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
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
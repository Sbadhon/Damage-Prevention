import axios from "axios";
import {
  Ticket,
  TicketStatus,
  WorkOrder,
  WorkOrderStatus,
  RiskAssessment,
  PagedResponse,
  Crew,
} from "../types";

// ---- Base URLs & Tenant ----

const TICKET_API_BASE =
  import.meta.env.VITE_TICKET_API_URL ?? "https://localhost:5030";
const SCHEDULING_API_BASE =
  import.meta.env.VITE_SCHEDULING_API_URL ?? "https://localhost:5076";
const RISK_API_BASE =
  import.meta.env.VITE_RISK_API_URL ?? "https://localhost:5003";

const TENANT_ID = import.meta.env.VITE_TENANT_ID ?? "acme-corp";

const tenantHeaders = {
  "X-Tenant-Id": TENANT_ID,
};

// Axios instances per bounded context (optional but clean)
const ticketClient = axios.create({
  baseURL: TICKET_API_BASE,
  headers: tenantHeaders,
});

const schedulingClient = axios.create({
  baseURL: SCHEDULING_API_BASE,
  headers: tenantHeaders,
});

const riskClient = axios.create({
  baseURL: RISK_API_BASE,
  headers: tenantHeaders,
});

// ---- API surface used by the UI ----

export interface ListParams {
  pageNumber: number;
  pageSize: number;
}

// NOTE: These assume your backend DTOs match the frontend types in src/types.ts.
// If your API shapes differ, add mapping functions here.

const api = {
  // --------- Tickets ---------

  listTickets: async (params: ListParams): Promise<PagedResponse<Ticket>> => {
    const res = await ticketClient.get<PagedResponse<Ticket>>("/api/tickets", {
      params,
    });
    return res.data;
  },

  getTicketById: async (id: string): Promise<Ticket> => {
    const res = await ticketClient.get<Ticket>(`/api/tickets/${id}`);
    return res.data;
  },

  createTicket: async (
    newTicket: Omit<Ticket, "ticketId" | "status" | "createdAt">
  ): Promise<Ticket> => {
    const body = {
      workType: newTicket.workType,
      address: newTicket.address,
      description: newTicket.description,
      lat: newTicket.lat ?? 0,
      lon: newTicket.lon ?? 0,
    };

    const res = await ticketClient.post("/api/tickets", body);
    return res.data as Ticket;
  },

  updateTicketStatus: async (
    ticketId: string,
    newStatus: TicketStatus,
    reason?: string // optional, for cancellation
  ): Promise<void> => {
    console.log(ticketId, newStatus);
    switch (newStatus) {
      case TicketStatus.Completed: // 3
        await ticketClient.post(`/api/tickets/${ticketId}/complete`);
        break;
      case TicketStatus.Cancelled: // 4
        await ticketClient.post(
          `/api/tickets/${ticketId}/cancel`,
          reason ? reason : null
        );
        break;
      default:
        await ticketClient.patch(`/api/tickets/${ticketId}`, {
          status: newStatus,
        });
        break;
    }
  },

  // --------- Work Orders (SchedulingSvc) ---------

  listWorkOrders: async (
    params: ListParams
  ): Promise<PagedResponse<WorkOrder>> => {
    const res = await schedulingClient.get<PagedResponse<WorkOrder>>(
      "/api/workorders",
      { params }
    );
    return res.data;
  },

  updateWorkOrderStatus: async (
    workOrderId: string,
    newStatus: WorkOrderStatus
  ): Promise<WorkOrder> => {
    const res = await schedulingClient.patch(
      `/api/workorders/${workOrderId}/status`,
      {
        status: newStatus,
      }
    );
    return res.data;
  },

  // --------- Risk (RiskSvc) ---------

  listRiskAssessments: async (
    params: ListParams
  ): Promise<PagedResponse<RiskAssessment>> => {
    const res = await riskClient.get<PagedResponse<RiskAssessment>>(
      "/api/risk",
      { params }
    );
    return res.data;
  },

  getRiskByTicketId: async (ticketId: string): Promise<RiskAssessment> => {
    const res = await riskClient.get<RiskAssessment>(
      `/api/risk/ticket/${ticketId}`
    );
    return res.data;
  },

  listCrews: async (): Promise<Crew[]> => {
    const res = await schedulingClient.get<Crew[]>("/api/workorders/crews");
    return res.data;
  },

  assignCrew: async (
    workOrderId: string,
    crewId: string
  ): Promise<WorkOrder> => {
    const res = await schedulingClient.put<WorkOrder>(
      `/api/workorders/${workOrderId}/assign-crew`,
      { crewId }
    );
    return res.data;
  },
};

export default api;

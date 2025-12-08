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

const API_GATEWAY_URL =
  import.meta.env.VITE_API_GATEWAY_URL ?? "http://localhost:5117";

const TENANT_ID = import.meta.env.VITE_TENANT_ID ?? "acme-corp";

const tenantHeaders = {
  "X-Tenant-Id": TENANT_ID,
};

const apiGatewayClient = axios.create({
  baseURL: API_GATEWAY_URL,
  headers: tenantHeaders,
});

export interface ListParams {
  pageNumber: number;
  pageSize: number;
}

const api = {
  // --------- Tickets (TicketSvc via Gateway) ---------

  listTickets: async (params: ListParams): Promise<PagedResponse<Ticket>> => {
    const res = await apiGatewayClient.get<PagedResponse<Ticket>>(
      "/api/tickets",
      { params }
    );
    return res.data;
  },

  getTicketById: async (id: string): Promise<Ticket> => {
    const res = await apiGatewayClient.get<Ticket>(`/api/tickets/${id}`);
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

    const res = await apiGatewayClient.post("/api/tickets", body);
    return res.data as Ticket;
  },

  updateTicketStatus: async (
    ticketId: string,
    newStatus: TicketStatus,
    reason?: string
  ): Promise<void> => {
    switch (newStatus) {
      case TicketStatus.Completed:
        await apiGatewayClient.post(`/api/tickets/${ticketId}/complete`);
        break;

      case TicketStatus.Cancelled:
        await apiGatewayClient.post(
          `/api/tickets/${ticketId}/cancel`,
          reason ? { reason } : null
        );
        break;

      default:
        await apiGatewayClient.patch(`/api/tickets/${ticketId}`, {
          status: newStatus,
        });
        break;
    }
  },

  // --------- Work Orders (SchedulingSvc via Gateway) ---------

  listWorkOrders: async (
    params: ListParams
  ): Promise<PagedResponse<WorkOrder>> => {
    const res = await apiGatewayClient.get<PagedResponse<WorkOrder>>(
      "/api/workorders",
      { params }
    );
    return res.data;
  },

  updateWorkOrderStatus: async (
    workOrderId: string,
    newStatus: WorkOrderStatus
  ): Promise<WorkOrder> => {
    const res = await apiGatewayClient.patch(
      `/api/workorders/${workOrderId}/status`,
      { status: newStatus }
    );
    return res.data;
  },

  listCrews: async (): Promise<Crew[]> => {
    const res = await apiGatewayClient.get<Crew[]>(
      "/api/workorders/crews"
    );
    return res.data;
  },

  assignCrew: async (
    workOrderId: string,
    crewId: string
  ): Promise<WorkOrder> => {
    const res = await apiGatewayClient.put<WorkOrder>(
      `/api/workorders/${workOrderId}/assign-crew`,
      { crewId }
    );
    return res.data;
  },

  // --------- Risk (RiskSvc via Gateway) ---------

  listRiskAssessments: async (
    params: ListParams
  ): Promise<PagedResponse<RiskAssessment>> => {
    const res = await apiGatewayClient.get<PagedResponse<RiskAssessment>>(
      "/api/risk",
      { params }
    );
    return res.data;
  },

  getRiskByTicketId: async (ticketId: string): Promise<RiskAssessment> => {
    const res = await apiGatewayClient.get<RiskAssessment>(
      `/api/risk/ticket/${ticketId}`
    );
    return res.data;
  },
};

export default api;

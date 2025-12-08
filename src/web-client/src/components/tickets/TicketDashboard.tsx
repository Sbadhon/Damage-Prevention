import React, { useState, useEffect, useCallback } from "react";
import { Ticket, TicketStatus, RiskAssessment } from "../../types";
import api from "../../services/api";
import { Pagination } from "../common/Pagination";
import { Modal } from "../common/Modal";
import { RiskBadge } from "../common/RiskBadge";
import { AddTicketForm } from "./AddTicketForm";
import { DashboardSummary } from "../common/DashboardSummary";
import { useAppDispatch, useAppSelector } from "../../hooks";
import {
  fetchTicketsPage,
  updateTicketStatusThunk,
  createTicketThunk,
  selectTicketsState,
  setPage,
} from "../../state/ticketsSlice";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import "leaflet/dist/leaflet.css";
import { TicketStatusBadge } from "../common/TicketStatusBadge";

const TicketDetails: React.FC<{
  ticket: Ticket;
  onStatusChange: (ticketId: string, newStatus: TicketStatus) => void;
}> = ({ ticket, onStatusChange }) => {
  const [risk, setRisk] = useState<RiskAssessment | null>(null);
  const [currentStatus, setCurrentStatus] = useState<TicketStatus>(
    ticket.status
  );

  const getAllowedTransitions = (
    current: TicketStatus,
    crewAssigned?: string | null
  ): TicketStatus[] => {
    switch (current) {
      case TicketStatus.Open:
        const transitions = [
          TicketStatus.Open,
          TicketStatus.Completed,
          TicketStatus.Cancelled,
        ];
        // Only allow "In Progress" if a crew is assigned
        if (crewAssigned) {
          transitions.splice(1, 0, TicketStatus.InProgress); // Insert "In Progress" after Open
        }
        return transitions;

      case TicketStatus.InProgress:
        return [
          TicketStatus.InProgress,
          TicketStatus.Completed,
          TicketStatus.Cancelled,
        ];

      case TicketStatus.Completed:
      case TicketStatus.Cancelled:
        return [current];

      default:
        return [current];
    }
  };

  const loadRisk = useCallback(async () => {
    try {
      const data = await api.getRiskByTicketId(ticket.ticketId);
      setRisk(data);
    } catch {
      setRisk(null);
    }
  }, [ticket.ticketId]);

  useEffect(() => {
    loadRisk();
  }, [loadRisk]);

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const newStatus = e.target.value as TicketStatus;
    setCurrentStatus(newStatus);
    onStatusChange(ticket.ticketId, newStatus);
  };

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <div className="space-y-2 text-gray-600 dark:text-gray-300">
            <p>
              <strong className="text-gray-500 dark:text-gray-400">
                Description:
              </strong>{" "}
              {ticket.description}
            </p>
            <p>
              <strong className="text-gray-500 dark:text-gray-400">
                Work Type:
              </strong>{" "}
              {ticket.workType}
            </p>
            <p>
              <strong className="text-gray-500 dark:text-gray-400">
                Address:
              </strong>{" "}
              {ticket.address}
            </p>
            <p>
              <strong className="text-gray-500 dark:text-gray-400">Lat:</strong>{" "}
              {ticket.lat}
            </p>
            <p>
              <strong className="text-gray-500 dark:text-gray-400">Lon:</strong>{" "}
              {ticket.lon}
            </p>
            <p>
              <strong className="text-gray-500 dark:text-gray-400">
                Created At:
              </strong>{" "}
              {new Date(ticket.createdAt).toLocaleString()}
            </p>
            <div className="flex items-center gap-2 mb-1">
              <strong className="text-gray-500 dark:text-gray-400">
                Status:
              </strong>{" "}
              <select
                value={currentStatus}
                onChange={handleStatusChange}
                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-teal-500 focus:border-teal-500 text-sm p-2.5"
              >
                {getAllowedTransitions(ticket.status, ticket.crewId).map(
                  (status) => {
                    const isInProgressWithoutCrew =
                      status === TicketStatus.InProgress && !ticket.crewId;
                    return (
                      <option
                        key={status}
                        value={status}
                        disabled={isInProgressWithoutCrew}
                        title={
                          isInProgressWithoutCrew ? "Assign a crew first" : ""
                        }
                      >
                        {status}
                      </option>
                    );
                  }
                )}
              </select>
            </div>
          </div>
        </div>
        <div>
          <div className="space-y-4">
            <div>
              <p>
                <strong className="text-gray-500 dark:text-gray-400">
                  Risk:
                </strong>{" "}
                {risk && <RiskBadge level={risk.level} />}
              </p>
              {risk ? (
                <>
                  <p>
                    <strong className="text-gray-500 dark:text-gray-400">
                      Score:
                    </strong>{" "}
                    {risk.score}
                  </p>
                  <p>
                    <strong className="text-gray-500 dark:text-gray-400">
                      Assessed at:
                    </strong>{" "}
                    {new Date(risk.assessedAt).toLocaleString()}
                  </p>
                </>
              ) : (
                <p className="text-sm text-gray-500 dark:text-gray-400">
                  Loading risk assessment…
                </p>
              )}
              {/* MAP — Only show when lat/lon are valid */}
              {ticket.lat !== 0 && ticket.lon !== 0 && (
                <div className="h-64 w-full mt-4">
                  <MapContainer
                    center={[ticket.lat, ticket.lon]}
                    zoom={15}
                    scrollWheelZoom={false}
                    className="h-full w-full rounded-md"
                  >
                    <TileLayer
                      url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                      attribution="&copy; OpenStreetMap"
                    />
                    <Marker position={[ticket.lat, ticket.lon]}>
                      <Popup>{ticket.address}</Popup>
                    </Marker>
                  </MapContainer>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export const TicketDashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const {
    items: tickets,
    loading,
    pageNumber,
    pageSize,
    totalCount,
    stats,
    error,
  } = useAppSelector(selectTicketsState);

  const [selectedTicket, setSelectedTicket] = useState<Ticket | null>(null);
  const [isAddModalOpen, setIsAddModalOpen] = useState(false);
  const [statusFilter, setStatusFilter] = useState<TicketStatus | "All">("All");
  const [searchTerm, setSearchTerm] = useState("");

  // Fetch tickets + stats when page changes
  useEffect(() => {
    dispatch(fetchTicketsPage({ pageNumber, pageSize }));
  }, [dispatch, pageNumber, pageSize]);

  const handlePageChange = (newPage: number) => {
    dispatch(setPage(newPage));
    dispatch(fetchTicketsPage({ pageNumber: newPage, pageSize }));
  };

  const handleStatusChange = async (
    ticketId: string,
    newStatus: TicketStatus
  ) => {
    await dispatch(updateTicketStatusThunk({ ticketId, newStatus }));
  };

  const handleAddTicket = async (
    newTicketData: Omit<Ticket, "ticketId" | "status" | "createdAt">
  ) => {
    await dispatch(createTicketThunk(newTicketData));
  };

  const filteredTickets = tickets.filter((t: Ticket) => {
    const matchesStatus =
      statusFilter === "All" ? true : t.status === statusFilter;
    const matchesSearch =
      !searchTerm ||
      t.ticketId.toLowerCase().includes(searchTerm.toLowerCase()) ||
      t.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
      t.address.toLowerCase().includes(searchTerm.toLowerCase());
    return matchesStatus && matchesSearch;
  });

  const dashboardSummaryStats =
    stats &&
    ({
      openTickets: {
        title: "Open Tickets",
        value: stats.tickets.open,
        color: "bg-blue-100 dark:bg-blue-900/40",
      },
      inProgressTickets: {
        title: "In Progress",
        value: stats.tickets.inProgress,
        color: "bg-yellow-100 dark:bg-yellow-500/20",
      },
      completedTickets: {
        title: "Completed",
        value: stats.tickets.completed,
        color: "bg-green-100 dark:bg-green-500/20",
      },
      highRisk: {
        title: "High/Critical Risk",
        value: stats.risks.high + stats.risks.critical,
        color: "bg-red-100 dark:bg-red-500/20",
      },
    } as Record<string, { title: string; value: number; color: string }>);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            Tickets
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Multi-tenant ticket dashboard with live risk &amp; status.
          </p>
        </div>
        <button
          onClick={() => setIsAddModalOpen(true)}
          className="px-4 py-2 text-sm font-medium text-white bg-teal-600 rounded-lg shadow hover:bg-teal-700"
        >
          + New Ticket
        </button>
      </div>

      {dashboardSummaryStats && (
        <DashboardSummary stats={dashboardSummaryStats} />
      )}

      <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl shadow-sm overflow-hidden">
        <div className="p-4 flex flex-col md:flex-row gap-3 md:items-center md:justify-between">
          <div className="flex gap-2">
            <select
              value={statusFilter}
              onChange={(e) =>
                setStatusFilter(e.target.value as TicketStatus | "All")
              }
              className="rounded-md border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm"
            >
              <option value="All">All Statuses</option>
              {Object.values(TicketStatus).map((status) => (
                <option key={status} value={status}>
                  {status}
                </option>
              ))}
            </select>
          </div>
          <div className="flex-1 md:max-w-xs">
            <input
              type="text"
              placeholder="Search by ID, description, or address..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="w-full rounded-md border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 text-sm px-3 py-2"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          {loading ? (
            <div className="p-6 text-center text-gray-500">
              Loading tickets…
            </div>
          ) : error ? (
            <div className="p-6 text-center text-red-500">{error}</div>
          ) : filteredTickets.length === 0 ? (
            <div className="p-6 text-center text-gray-500">
              No tickets found.
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
              <thead className="bg-gray-50 dark:bg-gray-800/80">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    ID
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Description
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Work Type
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Address
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Status
                  </th>
                  <th className="px-4 py-3 text-right font-medium text-gray-500 dark:text-gray-400">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {filteredTickets.map((ticket: Ticket) => (
                  <tr key={ticket.ticketId}>
                    <td className="px-4 py-3 whitespace-nowrap text-gray-900 dark:text-gray-100">
                      {ticket.ticketId}
                    </td>
                    <td className="px-4 py-3 max-w-xs truncate text-gray-700 dark:text-gray-200">
                      {ticket.description}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {ticket.workType}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {ticket.address}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      <TicketStatusBadge level={ticket.status} />
                    </td>
                    <td className="px-4 py-3 text-right">
                      <button
                        onClick={() => setSelectedTicket(ticket)}
                        className="inline-flex items-center px-3 py-1.5 rounded-lg text-xs font-medium bg-gray-100 dark:bg-gray-800 text-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700"
                      >
                        View details
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="p-4 border-t border-gray-100 dark:border-gray-800">
          <Pagination
            pageNumber={pageNumber}
            pageSize={pageSize}
            totalCount={totalCount}
            onPageChange={handlePageChange}
          />
        </div>
      </div>

      {selectedTicket && (
        <Modal
          isOpen={!!selectedTicket}
          onClose={() => setSelectedTicket(null)}
          title={`Ticket Details: ${selectedTicket.ticketId}`}
        >
          <TicketDetails
            ticket={selectedTicket}
            onStatusChange={handleStatusChange}
          />
        </Modal>
      )}

      <Modal
        isOpen={isAddModalOpen}
        onClose={() => setIsAddModalOpen(false)}
        title="Create New Ticket"
      >
        <AddTicketForm
          onAddTicket={handleAddTicket}
          onClose={() => setIsAddModalOpen(false)}
        />
      </Modal>
    </div>
  );
};

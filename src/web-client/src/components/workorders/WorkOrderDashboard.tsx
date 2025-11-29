import React, { useState, useEffect } from "react";
import { WorkOrder, WorkOrderStatus } from "../../types";
import { Pagination } from "../common/Pagination";
import { Modal } from "../common/Modal";
import { useAppDispatch, useAppSelector } from "../../hooks";

import {
  fetchWorkOrdersPage,
  selectWorkOrdersState,
  setPage,
  updateWorkOrderStatusThunk,
} from "../../state/workOrdersSlice";
import { DashboardSummary } from "../common/DashboardSummary";
import { WorkOrderDetails } from "./WorkOrderDetails";
import { WorkOrderStatusBadge } from "../common/WorkOrderStatusBadge";

export const WorkOrderDashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, loading, error, pageNumber, pageSize, totalCount } =
    useAppSelector(selectWorkOrdersState);

  const [selectedOrder, setSelectedOrder] = useState<WorkOrder | null>(null);

  useEffect(() => {
    dispatch(fetchWorkOrdersPage({ pageNumber, pageSize }));
  }, [dispatch, pageNumber, pageSize]);

  const handlePageChange = (newPage: number) => {
    dispatch(setPage(newPage));
    dispatch(fetchWorkOrdersPage({ pageNumber: newPage, pageSize }));
  };

  const handleStatusChange = async (
    workOrderId: string,
    newStatus: WorkOrderStatus
  ) => {
    await dispatch(updateWorkOrderStatusThunk({ workOrderId, newStatus }));
  };

  const getAllowedTransitions = (current: WorkOrderStatus): WorkOrderStatus[] => {
    switch (current) {
      case WorkOrderStatus.Pending:
        return [
          WorkOrderStatus.Pending,
          WorkOrderStatus.Assigned,
          WorkOrderStatus.Completed,
          WorkOrderStatus.Cancelled,
        ];
      case WorkOrderStatus.Assigned:
        return [
          WorkOrderStatus.Assigned,
          WorkOrderStatus.Completed,
          WorkOrderStatus.Cancelled,
        ];
      case WorkOrderStatus.Completed:
      case WorkOrderStatus.Cancelled:
        // Cannot transition away
        return [current];
      default:
        return [current];
    }
  };  
  const dashboardSummaryStats = {
    pending: {
      title: "Pending",
      value: items.filter((wo) => wo.status === WorkOrderStatus.Pending).length,
      color: "bg-blue-100 dark:bg-blue-500/20",
    },
    assigned: {
      title: "Assigned",
      value: items.filter((wo) => wo.status === WorkOrderStatus.Assigned)
        .length,
      color: "bg-yellow-100 dark:bg-yellow-500/20",
    },
    completed: {
      title: "Completed",
      value: items.filter((wo) => wo.status === WorkOrderStatus.Completed)
        .length,
      color: "bg-green-100 dark:bg-green-500/20",
    },
    cancelled: {
      title: "Cancelled",
      value: items.filter((wo) => wo.status === WorkOrderStatus.Cancelled)
        .length,
      color: "bg-red-100 dark:bg-red-500/20",
    },
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            Work Orders
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Scheduling view for all tenant work orders.
          </p>
        </div>
      </div>

      {dashboardSummaryStats && (
        <DashboardSummary stats={dashboardSummaryStats} />
      )}

      <div className="bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 rounded-xl shadow-sm overflow-hidden">
        <div className="overflow-x-auto">
          {loading ? (
            <div className="p-6 text-center text-gray-500">
              Loading work orders…
            </div>
          ) : error ? (
            <div className="p-6 text-center text-red-500">{error}</div>
          ) : items.length === 0 ? (
            <div className="p-6 text-center text-gray-500">
              No work orders found.
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
              <thead className="bg-gray-50 dark:bg-gray-800/80">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    ID
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Ticket
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Crew
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Scheduled At
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
                {items.map((wo) => (
                  <tr key={wo.workOrderId}>
                    <td className="px-4 py-3 text-gray-900 dark:text-gray-100">
                      {wo.workOrderId}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {wo.ticketId}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {wo.crewName}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {new Date(wo.scheduledAt).toLocaleString()}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      <WorkOrderStatusBadge
                        level={wo.status}
                      ></WorkOrderStatusBadge>
                    </td>
                    <td className="px-4 py-3 text-right">
                      <div className="inline-flex gap-2">
                        <button
                          onClick={() => setSelectedOrder(wo)}
                          className="px-3 py-1.5 rounded-lg text-xs font-medium bg-gray-100 dark:bg-gray-800 text-gray-800 dark:text-gray-200 hover:bg-gray-200 dark:hover:bg-gray-700"
                        >
                          Details
                        </button>
                        <select
                          value={wo.status}
                          onChange={(e) =>
                            handleStatusChange(
                              wo.workOrderId,
                              e.target.value as WorkOrderStatus
                            )
                          }
                          disabled={
                            wo.status === WorkOrderStatus.Completed ||
                            wo.status === WorkOrderStatus.Cancelled
                          }
                          className="text-xs rounded-md border-gray-300 dark:border-gray-700 bg-white dark:bg-gray-800 disabled:bg-gray-300 dark:disabled:bg-gray-700"
                        >
                          {getAllowedTransitions(wo.status).map((status) => (
                            <option key={status} value={status}>
                              {status}
                            </option>
                          ))}
                        </select>
                      </div>
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

      {selectedOrder && (
        <Modal
          isOpen={!!selectedOrder}
          onClose={() => setSelectedOrder(null)}
          title={`Work Order: ${selectedOrder.workOrderId}`}
        >
          <WorkOrderDetails
            workOrder={selectedOrder}
            onClose={() => setSelectedOrder(null)}
          />
        </Modal>
      )}
    </div>
  );
};

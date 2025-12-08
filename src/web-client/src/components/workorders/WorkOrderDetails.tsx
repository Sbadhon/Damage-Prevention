import React, { useState, useEffect } from "react";
import { WorkOrder, WorkOrderStatus } from "../../types";
import { useAppDispatch } from "../../hooks";
import { assignCrewThunk } from "../../state/workOrdersSlice";
import api from "../../services/api";
import { WorkOrderStatusBadge } from "../common/WorkOrderStatusBadge";

export interface Crew {
  crewId: string;
  crewName: string;
  specialty: string;
}

export const WorkOrderDetails: React.FC<{
  workOrder: WorkOrder;
  onClose: () => void;
}> = ({ workOrder, onClose }) => {
  const dispatch = useAppDispatch();
  const [selectedCrew, setSelectedCrew] = useState(workOrder.crewId ?? "");
  const [crews, setCrews] = useState<Crew[]>([]);
  const [loadingCrews, setLoadingCrews] = useState(false);
  const [assigning, setAssigning] = useState(false);

  const isEditable = true;

  useEffect(() => {
    const loadCrews = async () => {
      try {
        setLoadingCrews(true);
        const crewList = await api.listCrews();
        setCrews(crewList);
      } catch (err) {
        console.error("Failed to load crews", err);
      } finally {
        setLoadingCrews(false);
      }
    };

    loadCrews();
  }, []);

  const handleAssign = async () => {
    if (!selectedCrew) return;
    setAssigning(true);
    try {
      await dispatch(
        assignCrewThunk({
          workOrderId: workOrder.workOrderId,
          crewId: selectedCrew,
        })
      );

      // Call parent callback to close modal
      onClose();
    } catch (err) {
      console.error("Failed to assign crew", err);
    } finally {
      setAssigning(false);
    }
  };

  return (
    <div className="space-y-4 text-gray-900 dark:text-gray-100">
      <p>
        <strong className="text-gray-700 dark:text-gray-400">
          Work Order ID:
        </strong>{" "}
        {workOrder.workOrderId}
      </p>
      <p>
        <strong className="text-gray-700 dark:text-gray-400">
          Associated Ticket ID:
        </strong>{" "}
        {workOrder.ticketId}
      </p>
      <p>
        <strong className="text-gray-700 dark:text-gray-400">Crew:</strong>{" "}
        {workOrder.crewName || "Unassigned"}
      </p>
      <p>
        <strong className="text-gray-700 dark:text-gray-400">
          Created At:
        </strong>{" "}
        {new Date(workOrder.createdAt).toLocaleString()}
      </p>
      <p>
        <strong className="text-gray-700 dark:text-gray-400">
          Scheduled At:
        </strong>{" "}
        {new Date(workOrder.scheduledAt).toLocaleString()}
      </p>
      <p>
        <strong className="text-gray-700 dark:text-gray-400">Status</strong>{" "}
        <WorkOrderStatusBadge level={workOrder.status} />
      </p>
      {isEditable && (
        <div className="pt-6 border-t border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700/30 -mx-6 px-6 pb-6 mt-4">
          <h4 className="font-bold text-lg text-gray-900 dark:text-white mb-3 pt-4">
            {workOrder.crewId ? "Change Crew Assignment" : "Assign Field Crew"}
          </h4>

          <div className="flex flex-col sm:flex-row items-start sm:items-end gap-4">
            <div className="w-full sm:w-auto flex-1">
              <label className="block text-xs font-medium text-gray-500 dark:text-gray-400 mb-1">
                Select Crew Team
              </label>
              <select
                value={selectedCrew}
                onChange={(e) => setSelectedCrew(e.target.value)}
                disabled={loadingCrews}
                className="w-full bg-white dark:bg-gray-800 border border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-teal-500 focus:border-teal-500 text-sm p-2.5"
              >
                <option value="">
                  {loadingCrews ? "Loading crews..." : "-- Select a crew --"}
                </option>
                {crews.map((crew) => (
                  <option key={crew.crewId} value={crew.crewName}>
                    {crew.crewName} ({crew.specialty})
                  </option>
                ))}
              </select>
            </div>

            <button
              onClick={handleAssign}
              disabled={
                !selectedCrew || selectedCrew === workOrder.crewId || assigning || workOrder.status === WorkOrderStatus.Cancelled
              }
              className="w-full sm:w-auto px-6 py-2.5 text-sm font-bold text-white bg-teal-600 rounded-lg hover:bg-teal-700 disabled:bg-gray-400 disabled:cursor-not-allowed transition-colors shadow-sm"
            >
              {assigning
                ? "Saving..."
                : workOrder.crewId
                  ? "Update Assignment"
                  : "Assign Crew"}
            </button>
          </div>

          {workOrder.crewId && selectedCrew === workOrder.crewId && (
            <p className="text-xs text-gray-500 mt-2">
              Current crew is selected.
            </p>
          )}
        </div>
      )}
    </div>
  );
};

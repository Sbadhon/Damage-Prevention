
import React from 'react';
import { WorkOrderStatus } from '../../types';

interface WorkOrderStatusBadgeProps {
  level: WorkOrderStatus;
}

export const WorkOrderStatusBadge: React.FC<WorkOrderStatusBadgeProps> = ({ level }) => {
  const levelStyles: { [key in WorkOrderStatus]: string } = {
    [WorkOrderStatus.Completed]: 'bg-green-100 text-green-800 border border-green-300',   // clear “success” green
    [WorkOrderStatus.Pending]: 'bg-yellow-100 text-yellow-800 border border-yellow-300', // clear “waiting” yellow
    [WorkOrderStatus.Assigned]: 'bg-blue-100 text-blue-800 border border-blue-300',    // distinct “in-progress” blue
    [WorkOrderStatus.Cancelled]: 'bg-red-100 text-red-800 border border-red-300',      // strong “error/cancelled” red
  };

  return (
    <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${levelStyles[level]}`}>
      {level}
    </span>
  );
};

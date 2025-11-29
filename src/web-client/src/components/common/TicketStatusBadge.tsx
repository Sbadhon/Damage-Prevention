
import React from 'react';
import { TicketStatus } from '../../types';

interface TicketStatusBadgeProps {
  level: TicketStatus;
}

export const TicketStatusBadge: React.FC<TicketStatusBadgeProps> = ({ level }) => {
  const levelStyles: { [key in TicketStatus]: string } = {
    [TicketStatus.Completed]: 'bg-green-100 text-green-800 border border-green-300',   // clear “success” green
    [TicketStatus.InProgress]: 'bg-yellow-100 text-yellow-800 border border-yellow-300', // clear “waiting” yellow
    [TicketStatus.Open]: 'bg-blue-100 text-blue-800 border border-blue-300',    // distinct “in-progress” blue
    [TicketStatus.Cancelled]: 'bg-red-100 text-red-800 border border-red-300',      // strong “error/cancelled” red
  };

  return (
    <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${levelStyles[level]}`}>
      {level}
    </span>
  );
};


import React from 'react';
import { RiskLevel } from '../../types';

interface RiskBadgeProps {
  level: RiskLevel;
}

export const RiskBadge: React.FC<RiskBadgeProps> = ({ level }) => {
  const levelStyles: { [key in RiskLevel]: string } = {
    [RiskLevel.Low]: 'bg-green-100 text-green-800 border border-green-300',
    [RiskLevel.Medium]: 'bg-yellow-500/20 text-yellow-300 border border-yellow-500/30',
    [RiskLevel.High]: 'bg-orange-500/20 text-orange-300 border border-orange-500/30',
    [RiskLevel.Critical]: 'bg-red-500/20 text-red-300 border border-red-500/30',
  };

  return (
    <span className={`px-2.5 py-1 text-xs font-semibold rounded-full ${levelStyles[level]}`}>
      {level}
    </span>
  );
};

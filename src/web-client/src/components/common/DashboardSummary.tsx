import React from 'react';

interface StatCardProps {
  title: string;
  value: number;
  color: string;
}

const StatCard: React.FC<StatCardProps> = ({ title, value, color }) => {
  return (
    <div className={`bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-xl p-4 flex items-center shadow-sm`}>
      <div className={`p-3 rounded-full mr-4 ${color}`}>
      </div>
      <div>
        <p className="text-sm text-gray-500 dark:text-gray-400 font-medium">{title}</p>
        <p className="text-2xl font-bold text-gray-900 dark:text-white">{value}</p>
      </div>
    </div>
  );
};

interface DashboardSummaryProps {
    stats: {
        [key: string]: StatCardProps;
    }
}

export const DashboardSummary: React.FC<DashboardSummaryProps> = ({ stats }) => {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      {Object.entries(stats).map(([key, stat]) => (
        <StatCard key={key} {...stat} />
      ))}
    </div>
  );
};

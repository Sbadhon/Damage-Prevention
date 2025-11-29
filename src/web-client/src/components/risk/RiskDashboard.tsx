import React, { useEffect } from "react";
import { Pagination } from "../common/Pagination";
import { RiskBadge } from "../common/RiskBadge";
import { useAppDispatch, useAppSelector } from "../../hooks";
import { fetchRiskPage, selectRiskState, setPage } from "../../state/riskSlice";
import { RiskLevel } from "../../types";
import { DashboardSummary } from "../common/DashboardSummary";

export const RiskDashboard: React.FC = () => {
  const dispatch = useAppDispatch();
  const { items, loading, error, pageNumber, pageSize, totalCount } =
    useAppSelector(selectRiskState);

  useEffect(() => {
    dispatch(fetchRiskPage({ pageNumber, pageSize }));
  }, [dispatch, pageNumber, pageSize]);

  const handlePageChange = (newPage: number) => {
    dispatch(setPage(newPage));
    dispatch(fetchRiskPage({ pageNumber: newPage, pageSize }));
  };

  const dashboardSummaryStats = {
    highRisk: {
      title: "High Risk",
      value: items.filter((r) => r.level === RiskLevel.High).length,
      color: "bg-orange-100 dark:bg-orange-500/20",
    },
    criticalRisk: {
      title: "Critical Risk",
      value: items.filter((r) => r.level === RiskLevel.Critical).length,
      color: "bg-red-100 dark:bg-red-500/20",
    },
    mediumRisk: {
      title: "Medium Risk",
      value: items.filter((r) => r.level === RiskLevel.Medium).length,
      color: "bg-yellow-100 dark:bg-yellow-500/20",
    },
    lowRisk: {
      title: "Low Risk",
      value: items.filter((r) => r.level === RiskLevel.Low).length,
      color: "bg-green-100 dark:bg-green-500/20",
    },
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between mb-4">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white">
            Risk Assessments
          </h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            Risk view across tickets for this tenant.
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
              Loading risk assessments…
            </div>
          ) : error ? (
            <div className="p-6 text-center text-red-500">{error}</div>
          ) : items.length === 0 ? (
            <div className="p-6 text-center text-gray-500">
              No risk assessments found.
            </div>
          ) : (
            <table className="min-w-full divide-y divide-gray-200 dark:divide-gray-800 text-sm">
              <thead className="bg-gray-50 dark:bg-gray-800/80">
                <tr>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Risk ID
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Ticket
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Level
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Score
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Address
                  </th>
                  <th className="px-4 py-3 text-left font-medium text-gray-500 dark:text-gray-400">
                    Assessed At
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
                {items.map((risk) => (
                  <tr key={risk.riskId}>
                    <td className="px-4 py-3 text-gray-900 dark:text-gray-100">
                      {risk.riskId}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {risk.ticketId}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      <RiskBadge level={risk.level} />
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {risk.score.toFixed(2)}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {risk.address}
                    </td>
                    <td className="px-4 py-3 text-gray-700 dark:text-gray-200">
                      {new Date(risk.assessedAt).toLocaleString()}
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
    </div>
  );
};

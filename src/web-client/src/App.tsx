import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { Sidebar } from "./components/layout/Sidebar";
import { TicketDashboard } from "./components/tickets/TicketDashboard";
import { WorkOrderDashboard } from "./components/workorders/WorkOrderDashboard";
import { RiskDashboard } from "./components/risk/RiskDashboard";
import { Header } from "./components/layout/Header";

const App: React.FC = () => {
  return (
    <div className="flex flex-col h-screen font-sans bg-white text-black dark:bg-gray-900 dark:text-gray-100">
      <Header />
      <div className="flex flex-1 overflow-hidden bg-gray-100 dark:bg-gray-900">
        <Sidebar />
        <main className="flex-1 p-6 lg:p-8 overflow-auto">
          <Routes>
            <Route path="/" element={<Navigate to="/tickets" replace />} />
            <Route path="/tickets" element={<TicketDashboard />} />
            <Route path="/workorders" element={<WorkOrderDashboard />} />
            <Route path="/risk" element={<RiskDashboard />} />
            <Route path="*" element={<div>Page Not Found</div>} />
          </Routes>
        </main>
      </div>
    </div>
  );
};

export default App;

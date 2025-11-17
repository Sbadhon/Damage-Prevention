import React from 'react';
import { NavLink } from 'react-router-dom';

interface NavItemProps {
  to: string;
  label: string;
}

const NavItem: React.FC<NavItemProps> = ({ to, label }) => {
  return (
    <li className="my-2">
      <NavLink
        to={to}
        className={({ isActive }) =>
          [
            'flex items-center p-3 cursor-pointer rounded-lg transition-all duration-200',
            isActive
              ? 'bg-teal-500 text-white shadow-lg'
              : 'text-gray-500 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700 hover:text-gray-900 dark:hover:text-white',
          ].join(' ')
        }
      >
        <span className="ml-4 font-medium">{label}</span>
      </NavLink>
    </li>
  );
};

export const Sidebar: React.FC = () => {
  return (
    <aside className="w-64 bg-white dark:bg-gray-800 p-4 flex flex-col justify-between border-r border-gray-200 dark:border-gray-700">
      <div>
        <nav>
          <ul>
            <NavItem to="/tickets" label="Tickets" />
            <NavItem to="/workorders" label="Work Orders" />
            <NavItem to="/risk" label="Risk Assessments" />
          </ul>
        </nav>
      </div>
      <div className="space-y-2">
        <div className="text-center text-gray-500 dark:text-gray-400 text-xs">
          <p>&copy; 2025 Damage Prevention</p>
          <p>Version 1.1.0</p>
        </div>
      </div>
    </aside>
  );
};

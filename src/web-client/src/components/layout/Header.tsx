import React from 'react';
import { ThemeSwitcher } from './ThemeSwitcher';

export const Header: React.FC = () => {
  return (
    <header className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700 p-4 shadow-sm z-10">
      <div className="flex justify-between items-center">
        <div className="flex items-center">
            <h1 className="text-xl font-bold ml-3 text-gray-900 dark:text-white">Damage Prevention</h1>
        </div>
        <div className="flex justify-between items-center">
            <button className="bg-teal-600 mr-3 hover:bg-teal-700 text-white font-bold py-2 px-4 rounded-lg transition-colors flex items-center gap-2">
                <span>Login</span>
            </button>
            <ThemeSwitcher />
        </div>
      </div>
    </header>
  );
};

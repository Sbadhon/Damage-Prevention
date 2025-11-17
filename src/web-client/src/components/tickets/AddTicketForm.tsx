import React, { useState } from 'react';
import { Ticket } from '../../types';

interface AddTicketFormProps {
  onAddTicket: (newTicketData: Omit<Ticket, 'id' | 'status' | 'createdAt' | 'lat' | 'lon'>) => Promise<void>;
  onClose: () => void;
}

export const AddTicketForm: React.FC<AddTicketFormProps> = ({ onAddTicket, onClose }) => {
  const [description, setDescription] = useState('');
  const [workType, setWorkType] = useState('');
  const [address, setAddress] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!description || !workType || !address) {
      alert('Please fill out all fields.');
      return;
    }
    setIsSubmitting(true);
    await onAddTicket({ description, workType, address });
    // No need to set isSubmitting to false, as the component will unmount on success
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-600 dark:text-gray-300">Description</label>
        <input
          type="text"
          id="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          className="mt-1 block w-full bg-gray-100 dark:bg-gray-700 border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-teal-500 focus:border-teal-500 sm:text-sm p-2"
          required
        />
      </div>
      <div>
        <label htmlFor="workType" className="block text-sm font-medium text-gray-600 dark:text-gray-300">Work Type</label>
        <input
          type="text"
          id="workType"
          value={workType}
          onChange={(e) => setWorkType(e.target.value)}
          className="mt-1 block w-full bg-gray-100 dark:bg-gray-700 border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-teal-500 focus:border-teal-500 sm:text-sm p-2"
          placeholder="e.g., Locate fiber"
          required
        />
      </div>
      <div>
        <label htmlFor="address" className="block text-sm font-medium text-gray-600 dark:text-gray-300">Address</label>
        <input
          type="text"
          id="address"
          value={address}
          onChange={(e) => setAddress(e.target.value)}
          className="mt-1 block w-full bg-gray-100 dark:bg-gray-700 border-gray-300 dark:border-gray-600 rounded-md shadow-sm focus:ring-teal-500 focus:border-teal-500 sm:text-sm p-2"
          required
        />
      </div>
      <div className="flex justify-end gap-4 pt-4">
        <button
          type="button"
          onClick={onClose}
          className="px-4 py-2 text-sm font-medium text-gray-700 dark:text-gray-300 bg-gray-200 dark:bg-gray-600 rounded-md hover:bg-gray-300 dark:hover:bg-gray-500"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isSubmitting}
          className="px-4 py-2 text-sm font-medium text-white bg-teal-600 rounded-md hover:bg-teal-700 disabled:bg-gray-500"
        >
          {isSubmitting ? 'Submitting...' : 'Create Ticket'}
        </button>
      </div>
    </form>
  );
};

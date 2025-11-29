import React, { useState, useEffect } from 'react';
import { MapContainer, TileLayer, Marker, Popup } from 'react-leaflet';
import 'leaflet/dist/leaflet.css';
import L from 'leaflet';
import { Ticket } from '../../types';

// Fix default marker icon for Leaflet in React
delete (L.Icon.Default.prototype as any)._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon-2x.png',
  iconUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png',
  shadowUrl: 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-shadow.png',
});

interface AddTicketFormProps {
  onAddTicket: (newTicketData: Omit<Ticket, 'ticketId' | 'status' | 'createdAt' | 'lat' | 'lon'> & { lat: number; lon: number }) => Promise<void>;
  onClose: () => void;
}

export const AddTicketForm: React.FC<AddTicketFormProps> = ({ onAddTicket, onClose }) => {
  const [description, setDescription] = useState('');
  const [workType, setWorkType] = useState('');
  const [address, setAddress] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [coords, setCoords] = useState<{ lat: number; lon: number } | null>(null);
  const [loadingCoords, setLoadingCoords] = useState(false);

  // Function to get lat/lon using Nominatim
  const getLatLon = async (address: string) => {
    const url = `https://nominatim.openstreetmap.org/search?q=${encodeURIComponent(address)}&format=json&limit=1`;
    const response = await fetch(url, { headers: { 'User-Agent': 'your-app-name' } });
    const data = await response.json();
    if (data.length > 0) {
      return {
        lat: parseFloat(data[0].lat),
        lon: parseFloat(data[0].lon),
      };
    } else {
      throw new Error('Address not found');
    }
  };

  // Update map coordinates whenever address changes
  useEffect(() => {
    if (!address) {
      setCoords(null);
      return;
    }

    const timer = setTimeout(async () => {
      setLoadingCoords(true);
      try {
        const c = await getLatLon(address);
        setCoords(c);
      } catch {
        setCoords(null);
      } finally {
        setLoadingCoords(false);
      }
    }, 800); // debounce API call

    return () => clearTimeout(timer);
  }, [address]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!description || !workType || !address) {
      alert('Please fill out all fields.');
      return;
    }
  
    setIsSubmitting(true);
    try {
      await onAddTicket({
        description,
        workType,
        address,
        lat: coords ? coords.lat : 0,   // allow invalid address
        lon: coords ? coords.lon : 0,   // allow invalid address
      });
      onClose();
    } catch (error) {
      console.error(error);
      alert('Failed to create ticket. Please try again.');
      setIsSubmitting(false);
    }
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

      {loadingCoords && <p className="text-sm text-gray-500 dark:text-gray-400">Fetching coordinates...</p>}

      {coords && (
        <div className="h-64 w-full mt-2">
          <MapContainer center={[coords.lat, coords.lon]} zoom={15} scrollWheelZoom={false} className="h-full w-full rounded-md">
            <TileLayer
              url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>'
            />
            <Marker position={[coords.lat, coords.lon]}>
              <Popup>{address}</Popup>
            </Marker>
          </MapContainer>
        </div>
      )}

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

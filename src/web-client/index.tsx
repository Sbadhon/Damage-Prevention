
import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './src/App';
import { ThemeProvider } from './src/contexts/ThemeContext';
import 'leaflet/dist/leaflet.css';

const rootElement = document.getElementById('root');
if (!rootElement) {
  throw new Error("Could not find root element to mount to");
}

const root = ReactDOM.createRoot(rootElement);
root.render(
  <React.StrictMode>
     <ThemeProvider>
      <App />
    </ThemeProvider>
  </React.StrictMode>
);

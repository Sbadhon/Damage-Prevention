/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_TICKET_API_URL: string;
    readonly VITE_SCHEDULING_API_URL: string;
    readonly VITE_RISK_API_URL: string;
    readonly VITE_TENANT_ID: string;
  }
  
  interface ImportMeta {
    readonly env: ImportMetaEnv;
  }
  
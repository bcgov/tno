/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** Base path of the API (defaults to `/api`). */
  readonly VITE_API_URL?: string;
  /** Overrides `auth-server-url` from `/keycloak.json`. */
  readonly VITE_KEYCLOAK_AUTH_SERVER_URL?: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

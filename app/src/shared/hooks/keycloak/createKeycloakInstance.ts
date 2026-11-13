import Keycloak from 'keycloak-js';

import { IKeycloakConfig } from '.';

let instance: Promise<Keycloak> | undefined;

/**
 * Fetches the configuration file `/keycloak.json` and creates the page's Keycloak client for it.
 * Overrides file configuration with environment variables.
 * The client is created once per page: a second client would race the first to redeem the login
 * code (e.g. when React StrictMode mounts the app twice in development).
 * @returns Promise<KeycloakInstance>
 */
export const createKeycloakInstance = (initOptions?: Keycloak.KeycloakInitOptions) =>
  (instance ??= create(initOptions));

const create = async (initOptions?: Keycloak.KeycloakInitOptions) => {
  try {
    const response = await fetch('/keycloak.json');
    const data = await response.json();
    const config = data as IKeycloakConfig;

    const result = new Keycloak({
      url: import.meta.env.VITE_KEYCLOAK_AUTH_SERVER_URL ?? config['auth-server-url'],
      realm: config.realm,
      clientId: config.resource,
    });
    if (!!initOptions) result.init(initOptions);

    return result;
  } catch {
    const result = new Keycloak('./keycloak.json');
    if (!!initOptions) result.init(initOptions);

    return result;
  }
};

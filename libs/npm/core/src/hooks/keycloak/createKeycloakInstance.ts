import Keycloak, { KeycloakInstance } from 'keycloak-js';
import { IKeycloakConfig } from '.';

/**
 * Fetches the configuration file `/keycloak.json` and creates a KeycloakInstance for it.
 * Overrides file configuration with environment variables.
 * @returns Promise<KeycloakInstance>
 */
export const createKeycloakInstance = async () => {
  try {
    const response = await fetch('/keycloak.json');
    const data = await response.json();
    const config = data as IKeycloakConfig;

    return Keycloak({
      url: process.env.REACT_APP_KEYCLOAK_AUTH_SERVER_URL ?? config['auth-server-url'],
      realm: config.realm,
      clientId: config.resource,
    }) as KeycloakInstance;
  } catch {
    return Keycloak('./keycloak.json') as KeycloakInstance;
  }
};

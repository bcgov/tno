import { AuthClientError, AuthClientEvent } from '@react-keycloak/core/lib/index';
import { KeycloakInstance } from 'keycloak-js';
import { storeKeycloakReady, storeToken } from 'store/slices';
import { store } from 'store/store';

export const keycloakEventHandler = (keycloak: KeycloakInstance) => {
  const keycloakEventHandler = (
    eventType: AuthClientEvent,
    error?: AuthClientError | undefined,
  ) => {
    if (eventType === 'onAuthSuccess') {
      store.dispatch(storeToken(keycloak.token!));
    } else if (eventType === 'onAuthRefreshSuccess') {
      store.dispatch(storeToken(keycloak.token!));
    } else if (eventType === 'onAuthLogout' || eventType === 'onTokenExpired') {
      store.dispatch(storeToken(null));
    } else if (eventType === 'onReady') {
      store.dispatch(storeKeycloakReady(true));
    } else {
      //TODO: log error properly
      console.debug(`keycloak event: ${eventType} error ${error}`);
    }
  };
  return keycloakEventHandler;
};

export default keycloakEventHandler;

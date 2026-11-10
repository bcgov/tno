import Keycloak from 'keycloak-js';
import React from 'react';

import { SummonContext } from '..';
import { type KeycloakEvent } from './KeycloakContext';

/**
 * useKeycloakEventHandler, provides a hook which returns keycloak event handler.
 * These update the SummonContext with authentication information.
 * @returns Keycloak event handler.
 */
export const useKeycloakEventHandler = () => {
  const state = React.useContext(SummonContext);

  return (keycloak: Keycloak) => (eventType: KeycloakEvent, error?: unknown) => {
    if (eventType === 'onAuthSuccess') {
      state.setToken(keycloak.token);
    } else if (eventType === 'onAuthRefreshSuccess') {
      state.setToken(keycloak.token);
    } else if (eventType === 'onAuthLogout' || eventType === 'onTokenExpired') {
      state.setToken(null);
    } else if (eventType === 'onReady') {
      state.setAuthReady(true);
    } else {
      //TODO: log error properly
      console.debug(`keycloak event: ${eventType} error`, error);
    }
  };
};

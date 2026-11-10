import type Keycloak from 'keycloak-js';
import { type KeycloakInitOptions } from 'keycloak-js';
import React from 'react';

import { KeycloakContext, type KeycloakEvent } from './KeycloakContext';

export interface IKeycloakProviderProps {
  /** The Keycloak client to initialize and expose. */
  authClient: Keycloak;
  /** Options for `keycloak.init()`; `onLoad` defaults to 'check-sso'. Read once, on first mount. */
  initOptions?: KeycloakInitOptions;
  /** Rendered instead of the children until Keycloak has initialized. */
  LoadingComponent?: React.ReactElement;
  /** Called for every Keycloak lifecycle event. */
  onEvent?: (event: KeycloakEvent, error?: unknown) => void;
  children?: React.ReactNode;
}

// keycloak-js 26 throws if a client is initialized twice (e.g. a StrictMode remount).
const initializedClients = new WeakSet<Keycloak>();

const isAuthenticated = (client: Keycloak) => !!client.idToken && !!client.token;

/**
 * Initializes Keycloak and provides it to the component tree.
 * Replaces `ReactKeycloakProvider` from the unmaintained `@react-keycloak/web` with the same
 * behaviour: the same events, the same token refresh on expiry and the same loading gate.
 */
export const KeycloakProvider: React.FC<IKeycloakProviderProps> = ({
  authClient,
  initOptions,
  LoadingComponent,
  onEvent,
  children,
}) => {
  const [state, setState] = React.useState({ initialized: false, authenticated: false });
  const onEventRef = React.useRef(onEvent);
  onEventRef.current = onEvent;
  const initOptionsRef = React.useRef(initOptions);

  React.useEffect(() => {
    const update = (event: KeycloakEvent) => () => {
      onEventRef.current?.(event);
      const authenticated = isAuthenticated(authClient);
      setState((prev) =>
        prev.initialized && prev.authenticated === authenticated
          ? prev
          : { initialized: true, authenticated },
      );
    };
    const error = (event: KeycloakEvent) => (reason?: unknown) =>
      onEventRef.current?.(event, reason);

    authClient.onReady = update('onReady');
    authClient.onAuthSuccess = update('onAuthSuccess');
    authClient.onAuthError = error('onAuthError');
    authClient.onAuthRefreshSuccess = update('onAuthRefreshSuccess');
    authClient.onAuthRefreshError = error('onAuthRefreshError');
    authClient.onAuthLogout = update('onAuthLogout');
    authClient.onTokenExpired = () => {
      onEventRef.current?.('onTokenExpired');
      // A failed refresh is reported through onAuthRefreshError.
      authClient.updateToken(5).catch(() => {});
    };

    if (!initializedClients.has(authClient)) {
      initializedClients.add(authClient);
      authClient
        .init({ onLoad: 'check-sso', ...initOptionsRef.current })
        .catch(error('onInitError'));
    } else if (authClient.didInitialize) {
      setState({ initialized: true, authenticated: isAuthenticated(authClient) });
    }

    return () => {
      authClient.onReady = undefined;
      authClient.onAuthSuccess = undefined;
      authClient.onAuthError = undefined;
      authClient.onAuthRefreshSuccess = undefined;
      authClient.onAuthRefreshError = undefined;
      authClient.onAuthLogout = undefined;
      authClient.onTokenExpired = undefined;
    };
  }, [authClient]);

  if (LoadingComponent && !state.initialized) return LoadingComponent;

  return (
    <KeycloakContext.Provider value={{ initialized: state.initialized, keycloak: authClient }}>
      {children}
    </KeycloakContext.Provider>
  );
};

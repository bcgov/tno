import type Keycloak from 'keycloak-js';
import React from 'react';

/** Lifecycle events raised by `KeycloakProvider` (the same names `@react-keycloak` used). */
export type KeycloakEvent =
  | 'onReady'
  | 'onInitError'
  | 'onAuthSuccess'
  | 'onAuthError'
  | 'onAuthRefreshSuccess'
  | 'onAuthRefreshError'
  | 'onAuthLogout'
  | 'onTokenExpired';

export interface IKeycloakContext {
  /** Whether `keycloak.init()` has completed. */
  initialized: boolean;
  /** The Keycloak client. Undefined outside a `KeycloakProvider`. */
  keycloak?: Keycloak;
}

export const KeycloakContext = React.createContext<IKeycloakContext>({ initialized: false });

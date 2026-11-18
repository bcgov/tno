import type Keycloak from 'keycloak-js';
import React from 'react';

import { KeycloakContext } from './KeycloakContext';

/**
 * The Keycloak client and whether it has initialized.
 * Must be used inside a `KeycloakProvider`.
 */
export const useKeycloak = (): { initialized: boolean; keycloak: Keycloak } => {
  const { initialized, keycloak } = React.useContext(KeycloakContext);
  if (!keycloak) throw new Error('useKeycloak must be used inside a KeycloakProvider.');
  return { initialized, keycloak };
};

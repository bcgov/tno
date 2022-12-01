import Keycloak from 'keycloak-js';

import { Claim, Role } from '..';

/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloak {
  instance: Keycloak;
  initialized: boolean;
  authenticated?: boolean;
  getDisplayName: () => string;
  getUsername: () => string;
  getUserKey: () => string;
  hasRole(role?: Role | Array<Role>): boolean;
  hasClaim(claim?: Claim | Array<Claim>): boolean;
}

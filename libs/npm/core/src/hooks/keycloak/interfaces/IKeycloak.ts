import { Claim, Role } from '..';

/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloak {
  instance: any;
  authenticated?: boolean;
  getDisplayName: () => string;
  hasRole(role?: Role | Array<Role>): boolean;
  hasClaim(claim?: Claim | Array<Claim>): boolean;
}

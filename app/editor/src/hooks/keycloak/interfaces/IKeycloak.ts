import { IKeycloakUser } from '.';

/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloak {
  instance: any;
  authenticated?: boolean;
  userInfo?: IKeycloakUser;
  hasRole(role?: string | Array<string>): boolean;
  hasClaim(claim?: string | Array<string>): boolean;
}

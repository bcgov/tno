/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloak {
  instance: any;
  authenticated?: boolean;
  getDisplayName: () => string;
  hasRole(role?: string | Array<string>): boolean;
  hasClaim(claim?: string | Array<string>): boolean;
}

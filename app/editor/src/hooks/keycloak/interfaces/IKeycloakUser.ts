/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloakUser {
  displayName?: string;
  businessIdentifier: string;
  name?: string;
  preferred_businessIdentifier?: string;
  given_name?: string;
  family_name?: string;
  email?: string;
  roles: string[];
  groups: string[];
}

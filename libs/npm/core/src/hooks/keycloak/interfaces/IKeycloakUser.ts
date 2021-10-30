/**
 * IKeycloak interface, represents the keycloak object for the authenticated user.
 */
export interface IKeycloakUser {
  username?: string;
  display_name?: string;
  businessIdentifier?: string;
  preferred_businessIdentifier?: string;
  name?: string;
  given_name?: string;
  family_name?: string;
  email?: string;
  roles: string[];
  groups: string[];
}

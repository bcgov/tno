/**
 * IKeycloak interface, represents the keycloak object for the authenticated token.
 */
export interface IKeycloakToken {
  // Standard props
  exp?: number;
  iat?: number;
  auth_time?: number;
  iss?: string;
  jti?: string;
  aud?: string;
  sub?: string;
  typ?: string;
  azp?: string;
  scope?: string;
  nonce?: string;
  sid?: string;
  session_state?: string;

  // Custom Realm
  username?: string;
  businessIdentifier?: string;
  preferred_businessIdentifier?: string;
  groups?: string[];

  // Keycloak Gold Realm
  identity_provider?: string;
  preferred_username?: string;
  display_name?: string;
  name?: string;
  given_name?: string;
  family_name?: string;
  email?: string;
  email_verified?: boolean;
  client_roles?: string[];
  resource_access?: { [name: string]: { roles: string[] } };

  // IDIR
  idir_user_guid?: string;
  idir_username?: string;

  // Github
  github_username?: string;
  github_id?: string;

  // BCeID
  bceid_username?: string;
  bceid_user_guid?: string;

  // Azure Entra
  bcgov_username?: string;
  bcgov_guid?: string;
  user_principal_name?: string;
}

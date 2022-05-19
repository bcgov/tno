import { useKeycloak } from '@react-keycloak/web';

import { Claim, IKeycloak, IKeycloakUser, Role } from '.';

/**
 * Provides extension methods to interact with the `keycloak` object.
 */
export function useKeycloakWrapper(): IKeycloak {
  const { keycloak } = useKeycloak();
  const token = keycloak.tokenParsed as IKeycloakUser;

  /**
   * Determine if the user has the specified 'claim', or one of the specified 'claims'.
   * @param claim - The name of the claim
   */
  const hasClaim = (claim?: Claim | Array<Claim>): boolean => {
    return (
      claim !== undefined &&
      claim !== null &&
      (typeof claim === 'string'
        ? token?.roles?.includes(claim)
        : claim.some((c) => token?.roles?.includes(c)))
    );
  };

  /**
   * Determine if the user belongs to the specified 'role', or one of the specified 'roles'.
   * @param role - The role name or an array of role name
   */
  const hasRole = (role?: Role | Array<Role>): boolean => {
    return (
      role !== undefined &&
      role !== null &&
      (typeof role === 'string'
        ? token?.groups?.includes(role)
        : role.some((r) => token?.groups?.includes(r)))
    );
  };

  /**
   * Extract the display name from the token.
   * @returns User's display name.
   */
  const getDisplayName = () => {
    return token?.display_name ?? '';
  };

  /**
   * Extract the unique username of the user
   * @returns User's unique username
   */
  const getUsername = () => {
    return token?.username ?? '';
  };

  /**
   * Determine if the current use is a new account.
   * This is performed by checking if they have any claims.
   * @returns Whether the user is a new account.
   */
  const isApproved = () => {
    return hasClaim([Claim.editor, Claim.subscriber, Claim.administrator]);
  };

  return {
    instance: keycloak,
    authenticated: keycloak.authenticated,
    getUsername,
    getDisplayName,
    hasRole,
    hasClaim,
    isApproved,
  };
}

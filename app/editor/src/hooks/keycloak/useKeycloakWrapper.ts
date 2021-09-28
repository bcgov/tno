import { useKeycloak } from '@react-keycloak/web';

import { IKeycloak, IKeycloakUser } from '.';

/**
 * Provides extension methods to interact with the `keycloak` object.
 */
export function useKeycloakWrapper(): IKeycloak {
  const { keycloak } = useKeycloak();
  const userInfo = keycloak?.userInfo as IKeycloakUser;

  /**
   * Determine if the user has the specified 'claim'
   * @param claim - The name of the claim
   */
  const hasClaim = (claim?: string | Array<string>): boolean => {
    return (
      claim !== undefined &&
      claim !== null &&
      (typeof claim === 'string'
        ? userInfo?.roles?.includes(claim)
        : claim.some((c) => userInfo?.roles?.includes(c)))
    );
  };

  /**
   * Determine if the user belongs to the specified 'role'
   * @param role - The role name or an array of role name
   */
  const hasRole = (role?: string | Array<string>): boolean => {
    return (
      role !== undefined &&
      role !== null &&
      (typeof role === 'string'
        ? userInfo?.groups?.includes(role)
        : role.some((r) => userInfo?.groups?.includes(r)))
    );
  };

  return {
    instance: keycloak,
    authenticated: keycloak.authenticated,
    userInfo: userInfo,
    hasRole: hasRole,
    hasClaim: hasClaim,
  };
}

export default useKeycloakWrapper;

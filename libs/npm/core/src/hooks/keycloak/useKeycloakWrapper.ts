import { useKeycloak } from '@react-keycloak/web';
import React from 'react';

import { Claim, IKeycloak, IKeycloakUser, Role } from '.';

let token: IKeycloakUser = { roles: [], groups: [] };

/**
 * Provides extension methods to interact with the `keycloak` object.
 */
export function useKeycloakWrapper(): IKeycloak {
  const { initialized: ready, keycloak } = useKeycloak();

  const [initialized, setInitialized] = React.useState(ready);

  React.useEffect(() => {
    // For some reason the keycloak value doesn't update state.
    if (initialized) token = keycloak.tokenParsed as IKeycloakUser;
    setInitialized(initialized && !!keycloak.authenticated);
  }, [keycloak, initialized, keycloak.authenticated]);

  const controller = React.useMemo(
    () => ({
      /**
       * Determine if the user has the specified 'claim', or one of the specified 'claims'.
       * @param claim - The name of the claim
       */
      hasClaim: (claim?: Claim | Array<Claim>): boolean => {
        if (claim === undefined && !!token.roles.length) return true;
        return (
          claim !== undefined &&
          claim !== null &&
          (typeof claim === 'string'
            ? token.roles?.includes(claim)
            : claim.some((c) => token?.roles?.includes(c)))
        );
      },
      /**
       * Determine if the user belongs to the specified 'role', or one of the specified 'roles'.
       * @param role - The role name or an array of role name
       */
      hasRole: (role?: Role | Array<Role>): boolean => {
        if (role === undefined && !!token.groups.length) return true;
        return (
          role !== undefined &&
          role !== null &&
          (typeof role === 'string'
            ? token.groups?.includes(role)
            : role.some((r) => token?.groups?.includes(r)))
        );
      },
      /**
       * Extract the display name from the token.
       * @returns User's display name.
       */
      getDisplayName: () => {
        return token.display_name ?? '';
      },
      /**
       * Extract the unique username of the user
       * @returns User's unique username
       */
      getUsername: () => {
        return token.username ?? '';
      },
    }),
    [],
  );

  return {
    initialized,
    instance: keycloak,
    authenticated: keycloak.authenticated,
    ...controller,
  };
}

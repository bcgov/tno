import { useKeycloak } from '@react-keycloak/web';
import React from 'react';

import { Claim, IKeycloak, IKeycloakToken, Role } from '.';

let token: IKeycloakToken = { client_roles: [], groups: [] };

/**
 * Provides extension methods to interact with the `keycloak` object.
 */
export function useKeycloakWrapper(): IKeycloak {
  const { initialized: ready, keycloak } = useKeycloak();

  const [initialized, setInitialized] = React.useState(ready);

  if (ready) token = keycloak.tokenParsed as IKeycloakToken;

  React.useEffect(() => {
    // For some reason the keycloak value doesn't update state.
    setInitialized(ready && !!keycloak.authenticated);
  }, [keycloak, ready, keycloak.authenticated]);

  const controller = React.useMemo(
    () => ({
      /**
       * Determine if the user has the specified 'claim', or one of the specified 'claims'.
       * @param claim - The name of the claim
       */
      hasClaim: (claim?: Claim | Array<Claim>): boolean => {
        const client_roles = token?.client_roles;
        const resource_roles = token?.resource_access?.['mmi-app']?.roles;

        if (claim === undefined && (!!client_roles?.length || !!resource_roles?.length))
          return true;

        return (
          claim !== undefined &&
          claim !== null &&
          (typeof claim === 'string'
            ? (client_roles?.includes(claim) || resource_roles?.includes(claim)) ?? false
            : claim.some((c) => client_roles?.includes(c) || resource_roles?.includes(c)))
        );
      },
      /**
       * Determine if the user belongs to the specified 'role', or one of the specified 'roles'.
       * @param role - The role name or an array of role name
       */
      hasRole: (role?: Role | Array<Role>): boolean => {
        if (role === undefined && !!token?.groups?.length) return true;
        return (
          role !== undefined &&
          role !== null &&
          (typeof role === 'string'
            ? token?.groups?.includes(role) ?? false
            : role.some((r) => token?.groups?.includes(r)))
        );
      },
      /**
       * Extract the display name from the token.
       * @returns User's display name.
       */
      getDisplayName: () => {
        return token?.display_name ?? token?.name ?? '';
      },
      /**
       * Extract the unique username of the user
       * @returns User's unique username
       */
      getUserKey: () => {
        return token?.sub ?? '';
      },
      /**
       * Extract the unique username of the user
       * @returns User's unique username
       */
      getUsername: () => {
        return (
          token?.username ??
          token?.idir_username ??
          token?.github_username ??
          token?.bceid_username ??
          token?.bcgov_username ??
          token?.user_principal_name ??
          token?.preferred_username ??
          ''
        );
      },
      /**
       * Extract the unique UID of the user
       * @returns User's unique username
       */
      getUserUid: () => {
        return (
          token?.idir_user_guid ??
          token?.github_id ??
          token?.bceid_user_guid ??
          token?.bcgov_guid ??
          token?.sub ??
          ''
        );
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

import { Claim, useKeycloakWrapper } from 'tno-core';

/**
 * Hook to determine if a user is an admin based off their claims
 * @returns boolean value indicating if the user is an admin
 */
export const useAdmin = () => {
  const keycloak = useKeycloakWrapper();
  return keycloak.hasClaim(Claim.administrator);
};

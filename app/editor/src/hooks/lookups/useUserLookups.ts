import { IUserModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { useKeycloakWrapper } from 'tno-core';

/**
 * Lookup state cache can result in an array of users that are missing the current user, if it's the first time they've logged into the solution.
 * This hook ensures state contains them.
 * @returns Object containing the current user.
 */
export const useUserLookups = () => {
  const keycloak = useKeycloakWrapper();
  const [{ users }, { getUsers }] = useLookup();
  const [fetching, setFetching] = React.useState(false);

  const userId = users.find((u: IUserModel) => u.username === keycloak.getUsername())?.id ?? 0;

  React.useEffect(() => {
    // If for some reason the current user does not exist in the local list, go fetch a new list from the api.
    if (!userId && !fetching) {
      setFetching(true);
      getUsers(true);
    }
  }, [fetching, getUsers, userId]);

  return {
    userId,
    fetching,
  };
};

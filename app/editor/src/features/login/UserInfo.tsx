import React from 'react';
import { useApp } from 'store/hooks/app/useApp';
import { useKeycloakWrapper } from 'tno-core';

/**
 * UserInfo component provides a simple way to initialize user info within the application.
 * @returns Any empty component.
 */
export const UserInfo: React.FC = ({ children }) => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }, appHook, appStore] = useApp();

  React.useEffect(() => {
    if (keycloak.authenticated && userInfo === undefined) {
      appHook.getUserInfo();
    } else if (!keycloak.authenticated) {
      appStore.storeUserInfo();
    }
  }, [appHook, appStore, keycloak.authenticated, userInfo]);

  return <>{children}</>;
};

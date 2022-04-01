import React from 'react';
import { useApp } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';

/**
 * UserInfo component provides a simple way to initialize user info within the application.
 * @returns Any empty component.
 */
export const UserInfo: React.FC = ({ children }) => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }, app] = useApp();
  const [, appStore] = useAppStore();

  React.useEffect(() => {
    if (keycloak.authenticated && userInfo === undefined) {
      app.getUserInfo();
    } else if (!keycloak.authenticated) {
      appStore.storeUserInfo();
    }
  }, [app, appStore, keycloak.authenticated, userInfo]);

  return <>{children}</>;
};

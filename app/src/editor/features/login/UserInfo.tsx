import React from 'react';
import { useApp } from 'store/hooks';
import { useAppStore } from 'store/slices';
import { useKeycloakWrapper } from 'tno-core';

export interface IUserInfoProps {
  children?: React.ReactNode;
}

/**
 * UserInfo component provides a simple way to initialize user info within the application.
 * @returns Any empty component.
 */
export const UserInfo: React.FC<IUserInfoProps> = ({ children }) => {
  const keycloak = useKeycloakWrapper();
  const [, app] = useApp();
  const [, appStore] = useAppStore();

  const [init, setInit] = React.useState(true);

  React.useEffect(() => {
    if (keycloak.authenticated && init) {
      setInit(false);
      app.getUserInfo().catch(() => {});
    } else if (!keycloak.authenticated) {
      appStore.storeUserInfo();
      setInit(true);
    }
  }, [app, appStore, keycloak.authenticated, init]);

  return <>{children}</>;
};

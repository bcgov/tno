import React from 'react';

import { useApp } from '@/editor/store/hooks';
import { useAppStore } from '@/editor/store/slices';
import { useKeycloakWrapper } from '@/shared';

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

  // Whether user info still needs to be requested for the current sign-in. A ref, not state: the
  // effect runs again as soon as the request updates the store, before a state change would apply.
  const init = React.useRef(true);

  React.useEffect(() => {
    if (keycloak.authenticated && init.current) {
      init.current = false;
      app.getUserInfo().catch(() => {});
    } else if (!keycloak.authenticated && !init.current) {
      // Clear the user when they stop being authenticated (not on every render while signed out).
      init.current = true;
      appStore.storeUserInfo();
    }
  }, [app, appStore, keycloak.authenticated]);

  return <>{children}</>;
};

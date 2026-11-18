import React from 'react';

import {
  AccountAuthStateName,
  Button,
  Col,
  MessageTargetKey,
  useKeycloakWrapper,
  useModal,
} from '@/shared';
import { Modal } from '@/subscriber/components/modal';
import { useApiHub, useApp } from '@/subscriber/store/hooks';
import { useAppStore } from '@/subscriber/store/slices';

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
  const [{ userInfo }, appStore] = useAppStore();
  const { isShowing, toggle } = useModal();
  const hub = useApiHub();

  // Whether user info still needs to be requested for the current sign-in. A ref, not state: the
  // effect runs again as soon as the request updates the store, before a state change would apply.
  const init = React.useRef(true);

  React.useEffect(() => {
    if (keycloak.authenticated && init.current) {
      init.current = false;
      app
        .getUserInfo()
        .then((ui) => {
          if (ui.authState === AccountAuthStateName.Unauthorized) {
            toggle();
          }
        })
        .catch(() => {});
    } else if (!keycloak.authenticated && !init.current) {
      // Clear the user when they stop being authenticated (not on every render while signed out).
      init.current = true;
      appStore.storeUserInfo();
    }
  }, [app, appStore, keycloak.authenticated, toggle]);

  hub.useHubEffect(MessageTargetKey.Logout, () => {
    if (!isShowing) toggle();
    else keycloak.instance.logout();
  });

  return (
    <>
      {children}
      <Modal
        headerText="Account Access Detection"
        body={
          <Col flex="1">
            <p>Your account has exceeded its allowed connections.</p>
            <p>All active devices will automatically logoff.</p>
          </Col>
        }
        isShowing={isShowing}
        onClose={toggle}
        type="default"
        confirmText="Yes, Remove It"
        customButtons={
          <Button
            onClick={() => {
              hub.send('logout', [userInfo?.username, userInfo?.key]);
            }}
          >
            Logout
          </Button>
        }
      />
    </>
  );
};

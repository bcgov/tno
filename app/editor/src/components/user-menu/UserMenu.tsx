import { Button, ButtonVariant } from 'components';
import { useKeycloakWrapper } from 'hooks';

import { LogoutButton } from './LogoutButton';
import * as styled from './UserMenuStyled';

/**
 * UserMenu provides information and actions for the current user.
 * Unauthenticated:
 *  - login button.
 * Authenticated:
 *  - user's identity.
 *  - logout button
 * @returns UserMenu component.
 */
export const UserMenu = () => {
  const keycloak = useKeycloakWrapper();

  return (
    <div>
      {keycloak.authenticated ? (
        <styled.UserMenu>
          <div>{keycloak.getDisplayName()}</div>
          <LogoutButton onClick={() => keycloak.instance.logout()} size="20" />
        </styled.UserMenu>
      ) : (
        <Button variant={ButtonVariant.warning} onClick={() => keycloak.instance.login()}>
          Login
        </Button>
      )}
    </div>
  );
};

import { useKeycloakWrapper } from '../../hooks';
import { Button, ButtonVariant, LogoutButton } from '..';
import * as styled from './styled';

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
          <LogoutButton onClick={() => keycloak.instance.logout()} size={20} />
        </styled.UserMenu>
      ) : (
        <Button variant={ButtonVariant.warning} onClick={() => keycloak.instance.login()}>
          Login
        </Button>
      )}
    </div>
  );
};

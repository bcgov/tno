import { ScreenSizes } from 'components/layout/constants';
import { BiLogOut } from 'react-icons/bi';
import { FaUserCircle } from 'react-icons/fa';
import { Row, Show, useKeycloakWrapper, useWindowSize } from 'tno-core';

import * as styled from './styled';

/**
 * Component provides a logout button.
 */
export const UserProfile: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const { width } = useWindowSize();

  return (
    <styled.UserProfile>
      <div>{keycloak.getDisplayName()}</div>
      <Row onClick={() => keycloak.instance.logout()} className="logout">
        <Show visible={!!width && width > ScreenSizes.Mobile}>
          <FaUserCircle />
          Logout
        </Show>
        <Show visible={!!width && width < ScreenSizes.Mobile}>
          <BiLogOut className="logout-icon" />
        </Show>
      </Row>
    </styled.UserProfile>
  );
};

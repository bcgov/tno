import { ScreenSizes } from 'components/layout/constants';
import { BiLogOut } from 'react-icons/bi';
import { FaUserCircle } from 'react-icons/fa';
import { Tooltip } from 'react-tooltip';
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
      <div data-tooltip-id="my-info">{keycloak.getDisplayName()}</div>
      <Tooltip
        clickable
        variant="light"
        className="folder-menu"
        place="bottom"
        openOnClick
        style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="my-info"
      >
        <h1>My Profile</h1>
        <hr />
        <ul>
          <li>
            <a href="/colleagues">Colleagues</a>
          </li>
        </ul>
      </Tooltip>
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

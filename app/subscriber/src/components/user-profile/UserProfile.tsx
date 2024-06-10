import { ScreenSizes } from 'components/layout/constants';
import React from 'react';
import { BiLogOut } from 'react-icons/bi';
import { FaChevronCircleDown, FaQuestionCircle, FaUserCircle } from 'react-icons/fa';
import { Link, useNavigate } from 'react-router-dom';
import { Tooltip } from 'react-tooltip';
import { useUsers } from 'store/hooks';
import { useProfileStore } from 'store/slices';
import { Claim, Row, Show, useKeycloakWrapper, useWindowSize } from 'tno-core';

import * as styled from './styled';

/**
 * Component provides a logout button.
 */
export const UserProfile: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const { width } = useWindowSize();
  const [{ profile, impersonate }, { storeImpersonate }] = useProfileStore();
  const { getUser } = useUsers();
  const navigate = useNavigate();

  const [profileMenu, setProfileMenu] = React.useState(false);

  const isAdmin = keycloak.hasClaim(Claim.administrator);

  React.useEffect(() => {
    if (
      profile?.preferences?.impersonate &&
      profile?.preferences?.impersonate !== impersonate?.key
    ) {
      getUser()
        .then((user) => {
          storeImpersonate(user);
        })
        .catch(() => {});
    } else if (!profile?.preferences?.impersonate) {
      storeImpersonate(undefined);
    }
    // getUser will cause a double request.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [profile?.preferences?.impersonate, impersonate, storeImpersonate]);

  return (
    <styled.UserProfile>
      <Row
        direction="row"
        className="username-info"
        onClick={() => {
          navigate('/help');
        }}
      >
        <div>
          <Show visible={!!width && width > ScreenSizes.Mobile}>
            <FaQuestionCircle title="Help" size={15} />
          </Show>
        </div>
      </Row>
      <Row
        data-tooltip-id="my-info"
        direction="row"
        className={`username-info${impersonate ? ' impersonate' : ''}`}
      >
        <div>
          <Show visible={!!width && width > ScreenSizes.Mobile}>
            <FaChevronCircleDown size={15} />
          </Show>
          <div>{impersonate ? impersonate.displayName : keycloak.getDisplayName()}</div>
        </div>
      </Row>
      <Tooltip
        clickable
        variant="light"
        className="folder-menu"
        place="bottom"
        openOnClick
        closeOnEsc
        isOpen={profileMenu}
        setIsOpen={setProfileMenu}
        style={{ opacity: '1', boxShadow: '0 0 8px #464545', zIndex: '999' }}
        id="my-info"
      >
        <h1>My Profile</h1>
        <hr />
        <ul>
          <li>
            <Link to="/colleagues" onClick={(e) => setProfileMenu(false)}>
              Colleagues
            </Link>
          </li>
          {isAdmin && (
            <li>
              <Link to="/impersonation" onClick={(e) => setProfileMenu(false)}>
                Impersonation
              </Link>
            </li>
          )}
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

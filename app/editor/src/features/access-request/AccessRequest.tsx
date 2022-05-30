import { IUserModel, UserStatusName } from 'hooks/api-editor';
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useApp, useUsers } from 'store/hooks';
import { Row, Show, useKeycloakWrapper } from 'tno-core';

import { ApprovalDenied } from './ApprovalDenied';
import { ApprovalStatus } from './ApprovalStatus';
import { defaultUser } from './constants';
import { PreapprovedRequest } from './PreapprovedRequest';
import { RegisterRequest } from './RegisterRequest';
import * as styled from './styled';

export const AccessRequest: React.FC = (props) => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const location = useLocation();
  const users = useUsers();

  const [user, setUser] = React.useState<IUserModel>(defaultUser);

  const userId = userInfo?.id ?? 0;

  React.useEffect(() => {
    // The user has been approved, redirect back to home page.
    if (keycloak.isApproved() && location.pathname === '/welcome') navigate('/');
  });

  React.useEffect(() => {
    if (!!userId && user?.id !== userId) {
      users.getUser(userId).then((user) => {
        setUser(user);
      });
    }
  }, [user, users, userId]);

  return (
    <styled.AccessRequest>
      <Show visible={user.status === UserStatusName.Requested}>
        <ApprovalStatus user={user} setUser={setUser} />
      </Show>
      <Show visible={user.status === UserStatusName.Denied}>
        <ApprovalDenied user={user} setUser={setUser} />
      </Show>
      <Show
        visible={user.status !== UserStatusName.Requested && user.status !== UserStatusName.Denied}
      >
        <Row className="welcome">
          <h1>Welcome</h1>
          <p>
            Hello {keycloak.getDisplayName()}, If this is the first time signing into Today's News
            Online (TNO), you will need to request approval.
          </p>
        </Row>
        <Row gap="1em" justifyContent="space-evenly">
          <PreapprovedRequest user={user} setUser={setUser} />
          <RegisterRequest user={user} setUser={setUser} />
        </Row>
      </Show>
    </styled.AccessRequest>
  );
};

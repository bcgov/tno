import { UserStatusName } from 'hooks/api-editor';
import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Row, Show, useKeycloakWrapper } from 'tno-core';

import { ApprovalDenied } from './ApprovalDenied';
import { ApprovalStatus } from './ApprovalStatus';
import { PreapprovedRequest } from './PreapprovedRequest';
import { RegisterRequest } from './RegisterRequest';
import * as styled from './styled';

export const AccessRequest: React.FC = (props) => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const location = useLocation();

  React.useEffect(() => {
    // The user has been approved, redirect back to home page.
    if (keycloak.hasClaim() && location.pathname === '/welcome') navigate('/');
  });

  return (
    <styled.AccessRequest>
      <Show visible={userInfo?.status === UserStatusName.Requested}>
        <ApprovalStatus />
      </Show>
      <Show visible={userInfo?.status === UserStatusName.Denied}>
        <ApprovalDenied />
      </Show>
      <Show
        visible={
          userInfo?.status !== UserStatusName.Requested &&
          userInfo?.status !== UserStatusName.Denied
        }
      >
        <Row className="welcome">
          <h1>Welcome</h1>
          <p>
            Hello {keycloak.getDisplayName()}, If this is the first time signing into Media
            Monitoring Insights & Analysis, you will need to request approval.
          </p>
        </Row>
        <Row gap="1em" justifyContent="space-evenly">
          <PreapprovedRequest />
          <RegisterRequest />
        </Row>
      </Show>
    </styled.AccessRequest>
  );
};

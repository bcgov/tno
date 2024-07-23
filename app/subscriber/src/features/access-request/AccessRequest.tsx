import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Col, Show, useKeycloakWrapper, UserStatusName } from 'tno-core';

import { ApprovalDenied } from './ApprovalDenied';
import { ApprovalStatus } from './ApprovalStatus';
import * as styled from './styled';

/**
 * Component to submit access requests.
 * @returns Access request page.
 */
export const AccessRequest: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const [{ userInfo }] = useApp();
  const navigate = useNavigate();
  const location = useLocation();

  React.useEffect(() => {
    // The user has been approved, redirect back to home page.
    if (keycloak.hasClaim() && location.pathname === '/welcome') navigate('/');
  }, [keycloak, location.pathname, navigate]);

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
          userInfo &&
          [UserStatusName.Preapproved, UserStatusName.Approved].includes(userInfo.status)
        }
      >
        <Col alignContent="center">
          <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
          <Col className="main-box">
            <p className="top-bar-box">
              Welcome {keycloak.getDisplayName()}, If this is the first time signing into Media
              Monitoring Insights, your approval is being processed.
            </p>
            <p>
              If you are not redirected to the application, please contact{' '}
              <a href="mailto:scott.ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a>
            </p>
          </Col>
        </Col>
      </Show>
      <Show visible={userInfo?.status === UserStatusName.Activated}>
        <Col alignContent="center">
          <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
          <Col className="main-box">
            <p className="top-bar-box">
              Welcome {keycloak.getDisplayName()}. To gain access to Media Monitoring Insights you
              will need a paid subscription.
            </p>
            <p>
              Please contact <a href="mailto:scott.ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a>
            </p>
          </Col>
        </Col>
      </Show>
    </styled.AccessRequest>
  );
};

import React from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useApp } from 'store/hooks';
import { Col, Loader, Row, Show, useKeycloakWrapper, UserStatusName } from 'tno-core';

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
  const [showLoader, setShowLoader] = React.useState(false);

  React.useEffect(() => {
    // The user has been approved, redirect back to home page.
    if (keycloak.hasClaim() && location.pathname === '/welcome') navigate('/');
    setTimeout(() => {
      setShowLoader(true);
    }, 5000);
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
          userInfo?.status !== UserStatusName.Requested &&
          userInfo?.status !== UserStatusName.Denied
        }
      >
        <Col>
          <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
          <Loader visible={showLoader}>Loading</Loader>
          <Row className="containing-row">
            <Col className="main-box">
              <p className="top-bar-box">
                Welcome {keycloak.getDisplayName()}, If this is the first time signing into Media
                Monitoring Insights & Analysis, your approval is being processed.
              </p>
              <div className={'containing-box'}>
                <Col className="message-box">
                  <h1>Please wait while your user information is being set up.</h1>
                  <h2>You will be redirected automatically.</h2>
                </Col>
              </div>
            </Col>
          </Row>
        </Col>
      </Show>
    </styled.AccessRequest>
  );
};

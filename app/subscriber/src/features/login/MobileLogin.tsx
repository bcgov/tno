import React from 'react';
import { Button, Col, Row, Show, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

export interface IMobileLoginProps {
  login: (hint?: string) => void;
}

/**
 * Mobile login is the component that will be viewed when an unauthenticated user visits the site from a mobile device.
 * @param login The function to be called when the user clicks on the login button.
 * @returns MobileLogin component.
 */
export const MobileLogin: React.FC<IMobileLoginProps> = ({ login }) => {
  const keycloak = useKeycloakWrapper();
  const authority = keycloak.instance.authServerUrl?.replace(/\/$/, '') ?? window.location.href;
  const isLocal =
    new URL(authority).host.startsWith('localhost') ||
    new URL(authority).host.startsWith('host.docker.internal');

  return (
    <styled.MobileLogin>
      <Col className="mobile-view">
        <Row className="mobile-title">
          <div className="app-logo"></div>
        </Row>
        <Row className="containing-row">
          <Col className="main-box">
            <p className="top-bar-box">
              Media Monitoring is a paid service offered through the BC Government that allows
              subscribers to see British Columbia’s news at a glance.
            </p>
            <div className="containing-box centered-login-box">
              <Col className="login-box">
                <b>Login to your MMI account with your BCeID or IDIR: </b>
                <div>
                  <div className="buttons">
                    <Show visible={!isLocal}>
                      <Button
                        className="white idir-logo"
                        onClick={() => login(isLocal ? 'gcpe-oidc' : 'idir')}
                      >
                        IDIR
                      </Button>
                      <Button
                        className="white bceid-logo"
                        onClick={() => login(isLocal ? 'gcpe-oidc' : 'bceid-basic')}
                      ></Button>
                    </Show>
                    <Show visible={isLocal}>
                      <Button className="white" onClick={() => login()}>
                        Local
                      </Button>
                    </Show>
                    <br />
                    <p>
                      Learn more about obtaining a subscription contacting{' '}
                      <a href="mailto:Scott.Ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a>
                    </p>
                  </div>
                </div>
                <div className="footer" onClick={() => login()}>
                  <b>Copyright info:</b>
                  <p>
                    This account grants you access to copyrighted material for your own use. It does
                    not grant you permission to fix, copy, reproduce or archive any of the material
                    contained within. <br /> <br />
                    You cannot redistribute this information to anyone without violating your
                    copyright agreement.
                  </p>
                </div>
              </Col>
            </div>
          </Col>
        </Row>
      </Col>
      <img src="/assets/mm_logo.svg" alt="MM Logo" className="mm-logo" />
    </styled.MobileLogin>
  );
};

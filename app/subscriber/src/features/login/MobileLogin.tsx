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
          <img alt="MMIA Logo" src="/assets/MMinsights_logo_black.svg" />
        </Row>
        <Row>
          <p className="top-bar-box">
            Media Monitoring is a paid service offered through the BC Government that allows
            subscribers to see British Columbia’s news at a glance.
          </p>
        </Row>
        <Row>
          <div className="containing-box">
            <b>Key feautres: </b>
            <ul>
              <li>Aggregation of newspapers, radio shows, and online articles </li>
              <li>Transcription services</li>
              <li>BC’s top stories as they break</li>
              <li>Articles related to major stories</li>
            </ul>
            <a href="www.google.ca">Learn more about obtaining a subscription... </a>
            <Col className="login-box">
              <b>If you have a subscription, login here: </b>
              <div className="login-content">
                <div className="buttons">
                  <Show visible={!isLocal}>
                    <Button className="red" onClick={() => login(isLocal ? 'gcpe-oidc' : 'idir')}>
                      IDIR
                    </Button>
                    <Button
                      className="cyan"
                      onClick={() => login(isLocal ? 'gcpe-oidc' : 'bceid-basic')}
                    >
                      BCeID
                    </Button>
                  </Show>
                  <Show visible={isLocal}>
                    <Button className="red" onClick={() => login()}>
                      Local
                    </Button>
                  </Show>
                </div>
                <Row className="copyright">
                  <b>Copyright info:</b>
                  <p>
                    This account grants you access to copyrighted material for your own use. It does
                    not grant you permission to fix, copy, reproduce or archive any of the material
                    contained within. <br /> <br />
                    You cannot redistribute this information to anyone without violating your
                    copyright agreement.
                  </p>
                </Row>
              </div>
            </Col>
          </div>
        </Row>
      </Col>
    </styled.MobileLogin>
  );
};

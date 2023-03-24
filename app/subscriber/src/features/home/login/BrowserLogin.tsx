import { Button, Col, Row, Show, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

export interface IBrowserLoginProps {
  login: (hint?: string) => void;
}

/**
 * Browser login is the component that will be viewed when an unauthenticated user visits the site from a non mobile device.
 * @param login The function to be called when the user clicks on the login button.
 * @returns BrowserLogin component.
 */
export const BrowserLogin: React.FC<IBrowserLoginProps> = ({ login }) => {
  const keycloak = useKeycloakWrapper();
  const authority = keycloak.instance.authServerUrl?.replace(/\/$/, '') ?? window.location.href;
  const isLocal =
    new URL(authority).host.startsWith('localhost') ||
    new URL(authority).host.startsWith('host.docker.internal');
  return (
    <styled.BrowserLogin>
      <Row>
        <img alt="MMIA Logo" className="app-logo" src="/assets/mminsights_logo_black.svg" />

        <Row className="containing-row">
          <Col className="main-box">
            <p className="top-bar-box">
              Media Monitoring is a paid service offered through the BC Government that allows
              subscribers to see British Columbia’s news at a glance.
            </p>
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
                  <div onClick={() => login()} className="copyright">
                    <b>Copyright info:</b>
                    <p>
                      This account grants you access to copyrighted material for your own use. It
                      does not grant you permission to fix, copy, reproduce or archive any of the
                      material contained within. <br /> <br />
                      You cannot redistribute this information to anyone without violating your
                      copyright agreement.
                    </p>
                  </div>
                </div>
              </Col>
            </div>
          </Col>
          <Col className="alert-box">
            <div className="alert-containing-box">
              <b>System Notices</b>
              <br />
              <p>
                Service interruption Dec 29 - Jan 3. Stories will not be updated over the holidays.{' '}
                <br />
                <br />
                Jan 4th - Updates resume, business as usual.
              </p>
            </div>
          </Col>
        </Row>
      </Row>

      <img src="/assets/mm_logo.svg" alt="MM Logo" className="mm-logo" />
    </styled.BrowserLogin>
  );
};

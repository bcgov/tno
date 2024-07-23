import React from 'react';
import { Button, Col, Row, Show, useKeycloakWrapper } from 'tno-core';

import { Copyright } from './Copyright';
import * as styled from './styled';
import { SystemMessage } from './SystemMessage';

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

  const [showModal, setShowModal] = React.useState(false);

  return (
    <styled.BrowserLogin>
      <Col>
        <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
        <Row className="containing-row">
          <Col className="main-box">
            <p className="top-bar-box">
              Media Monitoring is a paid service offered through the BC Government that allows
              subscribers to see British Columbia’s news at a glance.
            </p>
            <div className={'containing-box'}>
              <Col className="login-box">
                <b>Login to your MMI account with your BCeID or IDIR: </b>
                <div>
                  <div className="buttons">
                    <Show visible={!isLocal}>
                      <Button
                        className="white idir-logo"
                        onClick={() => login('css-oidc')}
                      ></Button>
                      <Button className="white azure-logo" onClick={() => login('azure-entra')}>
                        Microsoft
                      </Button>
                      <Button className="white signIn" onClick={() => login()}>
                        Other
                      </Button>
                    </Show>
                    <Show visible={isLocal}>
                      <Button className="white" onClick={() => login()}>
                        Local
                      </Button>
                    </Show>
                    <br />
                    <span
                      className="modalOpen"
                      onClick={() => {
                        setShowModal(true);
                      }}
                    >
                      Learn more about obtaining a subscription...
                    </span>
                  </div>
                </div>
                <Copyright />
              </Col>
              <SystemMessage />
            </div>
          </Col>
        </Row>
      </Col>
      <Show visible={showModal}>
        <div id="myModal" className="modal">
          <div className="modal-content">
            <span
              className="close"
              onClick={() => {
                setShowModal(false);
              }}
            >
              &times;
            </span>
            <div>
              <p>
                <b>Thank you for your interest in Media Monitoring Insights.</b>
              </p>
              <p>
                <b>Key features:</b>
              </p>
              <ul>
                <li>Aggregation of newspapers, radio shows, and online articles</li>
                <li>Transcription services</li>
                <li>BC’s top stories as they break</li>
                <li>Articles related to major stories</li>
              </ul>
              <p>
                Access to this service is limited to Government of British Columbia staff as per our
                agreements with our media suppliers.
              </p>
              <br />
              <p>
                <b>
                  If you are eligible for access, email{' '}
                  <a href="mailto:Scott.Ryckman@gov.bc.ca">Scott.Ryckman@gov.bc.ca</a> to get set
                  up.
                </b>
              </p>
              <br />
              <p>
                Note: <br />
                There is a cost recovery charge in excess of $4,500 annually, please ensure that you
                have authorization for this purchase prior to emailing.
              </p>
            </div>
          </div>
        </div>
      </Show>
    </styled.BrowserLogin>
  );
};

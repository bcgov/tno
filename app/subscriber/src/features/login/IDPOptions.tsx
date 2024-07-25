import { Button } from 'components/button';
import { Col, Row, Show, useKeycloakWrapper } from 'tno-core';

import { IBrowserLoginProps } from './BrowserLogin';
import { Copyright } from './Copyright';
import { SystemMessage } from './SystemMessage';

export interface IIDPOptionsProps extends IBrowserLoginProps {
  /** Place below IDP button options */
  children?: React.ReactNode;
}

export const IDPOptions: React.FC<IIDPOptionsProps> = ({ login, children }) => {
  const keycloak = useKeycloakWrapper();
  const authority = keycloak.instance.authServerUrl?.replace(/\/$/, '') ?? window.location.href;
  const isLocal =
    new URL(authority).host.startsWith('localhost') ||
    new URL(authority).host.startsWith('host.docker.internal');

  return (
    <Row className="containing-row">
      <Col className="main-box">
        <p className="top-bar-box">
          Media Monitoring is a paid service offered through the BC Government that allows
          subscribers to see British Columbia’s news at a glance.
        </p>
        <div className="containing-box">
          <Col className="login-box">
            <b>
              Login to your MMI account with one of the following accounts: IDIR, Microsoft Azure
              account, or custom email account:
            </b>
            <div>
              <div className="buttons">
                <Show visible={!isLocal}>
                  <Button className="white idir-logo" onClick={() => login('css-oidc')}>
                    &nbsp;
                  </Button>
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
                {children}
              </div>
            </div>
            <Copyright />
          </Col>
          <SystemMessage />
        </div>
      </Col>
    </Row>
  );
};

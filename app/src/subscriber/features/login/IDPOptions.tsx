import { Button } from 'components/button';
import { Col, Row, Show, useKeycloakWrapper } from 'tno-core';

import { Copyright } from './Copyright';

export interface IIDPOptionsProps {
  /** Place below IDP button options */
  children?: React.ReactNode;
  /** Callback to login with the selected IDP */
  login: Function;
}

export const IDPOptions: React.FC<IIDPOptionsProps> = ({ login, children }) => {
  const keycloak = useKeycloakWrapper();
  const authority = keycloak.instance.authServerUrl?.replace(/\/$/, '') ?? window.location.href;
  const isLocal =
    new URL(authority).host.startsWith('localhost') ||
    new URL(authority).host.startsWith('host.docker.internal');
  return (
    <Row className="containing-row">
      <div className="containing-box">
        <Col className="button-box">
          <b className="login-info">
            Login to your MMI account with one of the following accounts: IDIR, Microsoft Azure
            account, or custom email account:
          </b>
          <div>
            <div className="buttons">
              <Show visible={!isLocal}>
                <Button className="white idir-logo" onClick={() => login('css-oidc')}>
                  &nbsp;
                </Button>
                {/* <Button className="white azure-logo" onClick={() => login('azure-entra')}>
                  Microsoft
                </Button> */}
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
      </div>
    </Row>
  );
};

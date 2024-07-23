import { Button, Col, Show, useKeycloakWrapper } from 'tno-core';

export const LoginPanel: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  const authority = keycloak.instance.authServerUrl?.replace(/\/$/, '') ?? window.location.href;

  const isLocal =
    new URL(authority).host.startsWith('localhost') ||
    new URL(authority).host.startsWith('host.docker.internal');

  const login = (hint?: string) => {
    const params = new URLSearchParams(window.location.search);
    const redirectTo = params.get('redirectTo');
    params.delete('redirectTo');
    const redirect = !redirectTo
      ? encodeURI(window.location.href)
      : encodeURI(
          window.location.href.split('?')[0].replace(window.location.pathname, redirectTo) +
            '?' +
            params.toString(),
        );
    keycloak.instance.login({ idpHint: hint, redirectUri: redirect, scope: 'openid' });
  };

  return (
    <div className="loginPanel">
      <div className="headerSection">
        <p>
          Welcome to <b>Media Monitoring Insights</b>
        </p>
        <Col alignItems="center" gap="1em">
          <p>Sign In</p>
          <Show visible={!isLocal}>
            <Button className="white azure-logo" onClick={() => login('css-oidc')}>
              IDIR
            </Button>
            <Button className="white azure-logo" onClick={() => login('azure-entra')}>
              Microsoft
            </Button>
            <Button className="signIn" onClick={() => login()}>
              Other
            </Button>
          </Show>
          <Show visible={isLocal}>
            <Button className="signIn" onClick={() => login()}>
              Local
            </Button>
          </Show>
        </Col>
      </div>
      <p className="copyright" style={{ color: '#AAAAAA' }}>
        COPYRIGHT: This account grants you access to copyrighted material for your own use. It does
        not grant you permission to fix, copy, reproduce or archive any of the material contained
        within. You cannot redistribute this information to anyone without violating your copyright
        agreement.
      </p>
    </div>
  );
};

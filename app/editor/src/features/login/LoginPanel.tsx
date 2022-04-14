import { Button, useKeycloakWrapper } from 'tno-core';

export const LoginPanel: React.FC = () => {
  const keycloak = useKeycloakWrapper();

  console.debug(keycloak);

  const login = () => {
    const instance = keycloak.instance;
    const authority = instance.authServerUrl.replace(/\/$/, '');
    const authUrl = `${authority}/realms/${instance.realm}`;
    const redirect = encodeURI(window.location.href);
    window.location.href = `${authUrl}/protocol/openid-connect/auth?client_id=${instance.clientId}&redirect_uri=${redirect}&response_mode=fragment&response_type=code&scope=openid&kc_idp_hint=idir`;
  };

  return (
    <div className="loginPanel">
      <div className="headerSection">
        <p>
          Welcome to <b>TNO News</b>
        </p>
        <Button className="signIn" onClick={login}>
          Sign In
        </Button>
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

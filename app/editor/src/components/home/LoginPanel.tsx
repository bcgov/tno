import { Button, useKeycloakWrapper } from 'tno-core';

export const LoginPanel: React.FC = () => {
  const keycloak = useKeycloakWrapper();
  return (
    <div className="loginPanel">
      <div className="headerSection">
        <p>
          Welcome to <b>TNO News</b>
        </p>
        <Button className="signIn" onClick={() => keycloak.instance.login()}>
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

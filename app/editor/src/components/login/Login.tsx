import { useKeycloakWrapper } from 'hooks';
import { useHistory } from 'react-router-dom';

export const Login = () => {
  const keycloak = useKeycloakWrapper();
  const history = useHistory();

  if (keycloak.authenticated) {
    history.push('/');
  }
  return <button onClick={() => keycloak.instance.login()}>Login</button>;
};

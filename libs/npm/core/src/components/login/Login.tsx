import { useHistory } from 'react-router-dom';

import { useKeycloakWrapper } from '../../hooks';

/**
 * Login will display content for an anonymous user.
 * If the user is already authenticated it will redirect to the home route.
 * @returns Login component.
 */
export const Login = () => {
  const keycloak = useKeycloakWrapper();
  const history = useHistory();

  if (keycloak.authenticated) {
    history.push('/');
  }
  return <div>Anonymous user</div>;
};

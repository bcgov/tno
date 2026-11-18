import { Navigate } from 'react-router';

import { useKeycloakWrapper } from '@/shared';

/**
 * Login will display content for an anonymous user.
 * If the user is already authenticated it will redirect to the home route.
 * @returns Login component.
 */
const Login = () => {
  const keycloak = useKeycloakWrapper();

  if (keycloak.authenticated) {
    return <Navigate to="/" />;
  }
  return <div>Anonymous user</div>;
};

export default Login;

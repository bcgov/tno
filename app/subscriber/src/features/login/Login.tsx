import { Navigate } from 'react-router-dom';
import { useKeycloakWrapper } from 'tno-core';

/**
 * Login will display content for an anonymous user.
 * If the user is already authenticated it will redirect to the home route.
 * @returns Login component.
 */
export const Login = () => {
  const keycloak = useKeycloakWrapper();

  if (keycloak.authenticated) {
    return <Navigate to="/" />;
  }
  return <div>Anonymous user</div>;
};

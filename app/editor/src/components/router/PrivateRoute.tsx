import { useKeycloakWrapper } from 'hooks';
import { Redirect, Route, RouteProps } from 'react-router-dom';

interface IPrivateRouteProps extends RouteProps {
  component: React.ComponentType<any>;
}

export const PrivateRoute = ({ component: Component, ...rest }: IPrivateRouteProps) => {
  const keycloak = useKeycloakWrapper();
  return (
    <Route
      {...rest}
      render={(routeProps) => {
        console.debug('keycloak', keycloak);
        if (!keycloak.authenticated) {
          return <Redirect to="/login" />;
        }
        return <Component {...routeProps} />;
      }}
    ></Route>
  );
};

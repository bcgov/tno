import { Redirect, Route, RouteProps } from 'react-router-dom';

import { Claim, Role, useKeycloakWrapper } from '../../hooks';

interface IPrivateRouteProps extends RouteProps {
  /**
   * The component to load if the route is active.
   * You can use children elements instead.
   */
  component?: React.ComponentType<any>;
  /**
   * A role the user belongs to.
   */
  roles?: Role | Array<Role>;
  /**
   * A claim the user has.
   */
  claims?: Claim | Array<Claim>;
}

/**
 * PrivateRoute provides a way to only show menu items for authenticated users.
 * @param param0 Route element attributes.
 * @returns PrivateRoute component.
 */
export const PrivateRoute = ({
  component: Component,
  claims,
  roles,
  children,
  ...rest
}: IPrivateRouteProps) => {
  const keycloak = useKeycloakWrapper();
  return (
    <Route
      {...rest}
      render={(routeProps) => {
        if (
          !keycloak.authenticated ||
          (!!claims && !keycloak.hasClaim(claims)) ||
          (!!roles && !keycloak.hasRole(roles))
        ) {
          return <Redirect to="/login" />;
        } else {
          if (Component) return <Component {...routeProps} />;
          else return children;
        }
      }}
    ></Route>
  );
};

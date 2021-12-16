import { Navigate, RouteProps } from 'react-router-dom';

import { Claim, Role, useKeycloakWrapper } from '../../hooks';

/**
 * PrivateRoute properties.
 */
interface IPrivateRouteProps extends RouteProps {
  /**
   * The component to load if the route is active.
   * You can use children elements instead.
   */
  element?: React.ReactElement | null;
  /**
   * A role the user belongs to.
   */
  roles?: Role | Array<Role>;
  /**
   * A claim the user has.
   */
  claims?: Claim | Array<Claim>;
  /**
   * The path to redirect to if user is unauthorized.
   */
  redirectTo: string;
}

/**
 * PrivateRoute provides a way to only show menu items for authenticated users.
 * @param param0 Route element attributes.
 * @returns PrivateRoute component.
 */
export const PrivateRoute = ({
  redirectTo,
  claims,
  roles,
  element,
  children,
}: IPrivateRouteProps) => {
  const keycloak = useKeycloakWrapper();

  if (
    !keycloak.authenticated ||
    (!!claims && !keycloak.hasClaim(claims)) ||
    (!!roles && !keycloak.hasRole(roles))
  ) {
    return <Navigate to={redirectTo} />;
  }

  return element ? element : <>{children}</>;
};

import { Navigate } from 'react-router-dom';
import { Claim, NotAuthorized, Role, useKeycloakWrapper } from 'tno-core';

/**
 * PrivateRoute properties.
 */
interface IPrivateRouteProps {
  /**
   * The path to redirect to if user is unauthorized.
   * Default value is '/login'.
   */
  redirectTo?: string;
  /**
   * A role the user belongs to.
   */
  roles?: Role | Array<Role>;
  /**
   * A claim the user has.
   */
  claims?: Claim | Array<Claim>;
  /**
   * The element to load if authorized.
   */
  element?: React.ReactElement | null;
  /**
   * The children elements to load if authorized.
   */
  children?: React.ReactNode;
}

/**
 * PrivateRoute provides a way to only show menu items for authenticated users.
 * @param param0 Route element attributes.
 * @returns PrivateRoute component.
 */
export const PrivateRoute = ({
  redirectTo = '/login',
  claims,
  roles,
  element,
  children,
}: IPrivateRouteProps) => {
  const keycloak = useKeycloakWrapper();

  if (!keycloak.authenticated) {
    return <Navigate to={redirectTo} />;
  } else if (!keycloak.hasClaim()) {
    return <Navigate to="/welcome" />;
  } else if ((!!claims && !keycloak.hasClaim(claims)) || (!!roles && !keycloak.hasRole(roles))) {
    return (
      <div>
        <NotAuthorized />
      </div>
    );
  }
  return element ? element : <>{children}</>;
};

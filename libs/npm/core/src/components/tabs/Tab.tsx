import { useLocation, useNavigate } from 'react-router-dom';

import { Claim, useKeycloakWrapper } from '../../hooks/keycloak';
import * as styled from './styled';
import { isActivePath } from './utils';

export interface ITabProps extends React.HTMLProps<HTMLButtonElement> {
  /**
   * choose the tab label
   */
  label?: string;
  /**
   * prop used to determine whether the tab is active
   */
  active?: boolean;
  /**
   * An array of paths that if match will make the tab active.
   */
  activePaths?: string[];
  /**
   * Whether the path must be exact to make tab active.
   */
  exact?: boolean;
  /**
   * the path the item will navigate you to
   */
  navigateTo?: string;
  /**
   * The user requires at least one of the specified claims to see this nav item.
   */
  claim?: Claim | Claim[];
}

/**
 * The individual item that will appear in the navigation bar, on click it will navigate to desired path and will use the applications
 * current path to determine whether it is active or not.
 * @param props Component properties.
 * @returns Tab component.
 */
export const Tab: React.FC<ITabProps> = ({
  label,
  navigateTo,
  claim,
  children,
  className,
  active = false,
  activePaths = [],
  exact = false,
  onClick,
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const keycloak = useKeycloakWrapper();
  const hasClaim = !claim || keycloak.hasClaim(claim);

  let isActive =
    active ||
    isActivePath(location.pathname, navigateTo, exact) ||
    activePaths.some((p) => isActivePath(location.pathname, p, exact));

  return hasClaim ? (
    <styled.Tab
      onClick={(e) => (onClick ? onClick(e as any) : navigate(navigateTo!!))}
      active={isActive}
      className={`${className ?? 'tab'}`}
    >
      <span>{label}</span>
      {children}
    </styled.Tab>
  ) : null;
};

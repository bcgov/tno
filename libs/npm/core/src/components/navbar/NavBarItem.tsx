import { useLocation, useNavigate } from 'react-router-dom';

import { Claim, useKeycloakWrapper } from '../../hooks/keycloak';
import * as styled from './styled';
import { isActive } from './utils';

export interface INavBarItemProps extends React.HTMLProps<HTMLButtonElement> {
  /**
   * choose the tab label
   */
  label?: string;
  /**
   * prop used to determine whether the tab is active
   */
  active?: boolean;
  /**
   * the path the item will navigate you to
   */
  navigateTo?: string;
  /**
   * The user requires at least one of the specified claims to see this nav item.
   */
  claim?: Claim | Claim[];
  /**
   * Whether the current location path needs to be an exact match with the navigateTo to be active.
   */
  exact?: boolean;
  /**
   * Identify which tab is currently being hovered
   */
  activeHoverTab?: string;
}

/**
 * The individual item that will appear in the navigation bar, on click it will navigate to desired path and will use the applications
 * current path to determine whether it is active or not.
 * @param props1 Component properties
 * @returns styled navigation bar item
 */
export const NavBarItem: React.FC<INavBarItemProps> = ({
  label,
  navigateTo,
  claim,
  exact = false,
  activeHoverTab,
}) => {
  const location = useLocation();
  const navigate = useNavigate();
  const keycloak = useKeycloakWrapper();
  const hasClaim = !claim || keycloak.hasClaim(claim);

  return hasClaim ? (
    <styled.NavBarItem
      onClick={() => navigate(navigateTo!!)}
      active={isActive(location.pathname, navigateTo, exact, activeHoverTab, label)}
    >
      {label}
    </styled.NavBarItem>
  ) : null;
};

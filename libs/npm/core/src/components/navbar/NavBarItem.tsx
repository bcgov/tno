import { useLocation, useNavigate } from 'react-router-dom';

import { Claim, useKeycloakWrapper } from '../../hooks/keycloak';
import * as styled from './styled';
import { isActive } from './utils';

export interface INavBarItem {
  /** The unique key to identify this item. */
  uid?: string;
  /**
   * choose the tab label
   */
  label?: React.ReactNode;
  /**
   * the path the item will navigate you to
   */
  navigateTo?: string;
}

export interface INavBarItemProps
  extends Omit<React.HTMLProps<HTMLButtonElement>, 'label'>,
    INavBarItem {
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
  /**
   * The level of the navigation item (0 = main menu).
   */
  level?: number;
}

export interface INavBarItemPropsAndEvents extends Omit<INavBarItemProps, 'onClick'> {
  /**
   * When this navbar item is selected.
   */
  onClick?: (item: INavBarItem) => boolean;
}

/**
 * The individual item that will appear in the navigation bar, on click it will navigate to desired path and will use the applications
 * current path to determine whether it is active or not.
 * @param props1 Component properties
 * @returns styled navigation bar item
 */
export const NavBarItem: React.FC<INavBarItemPropsAndEvents> = ({
  uid,
  label,
  navigateTo,
  claim,
  exact = false,
  activeHoverTab,
  level = 0,
  onClick = () => true,
}) => {
  const location = useLocation();
  const navigate = useNavigate();
  const keycloak = useKeycloakWrapper();
  const hasClaim = !claim || keycloak.hasClaim(claim);
  const key = uid ?? (typeof label === 'string' ? label : '');

  return hasClaim ? (
    <styled.NavBarItem
      onClick={(e) => {
        const nav = onClick?.({ uid: key, navigateTo });
        if (nav && navigateTo) {
          if (!e.ctrlKey) navigate(navigateTo);
          else window.open(navigateTo, '_blank');
        }
      }}
      active={isActive(location.pathname, navigateTo, exact, activeHoverTab, key)}
      level={level}
    >
      {label}
    </styled.NavBarItem>
  ) : null;
};

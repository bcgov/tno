import { useLocation, useNavigate } from 'react-router-dom';
import { Claim, useKeycloakWrapper } from 'tno-core';

import * as styled from './styled';

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
 * @param label the text to appear on the tab in the navigation bar
 * @param navigateTo determine the path the item will navigate to onClick
 * @returns styled navigation bar item
 */
export const Tab: React.FC<ITabProps> = ({
  label,
  navigateTo,
  claim,
  children,
  className,
  active = false,
  exact = false,
  onClick,
}) => {
  const navigate = useNavigate();
  const location = useLocation();
  const keycloak = useKeycloakWrapper();
  const hasClaim = !claim || keycloak.hasClaim(claim);

  let isActive =
    active ||
    (navigateTo
      ? exact
        ? location.pathname.endsWith(navigateTo)
        : location.pathname.includes(navigateTo)
      : false);

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

import { Menu as HMenu } from '@headlessui/react';
import React, { HTMLAttributes } from 'react';
import { IconType } from 'react-icons/lib';

import { Claim, Role, useKeycloakWrapper } from '../../hooks';
import { MenuStatus } from '.';
import * as styled from './styled';
import { isInViewport } from 'utils';

interface IMenuGroupProps extends HTMLAttributes<HTMLElement> {
  /**
   * The text label of the button.
   */
  label: string;
  /**
   * The menu variant display option.
   */
  status: MenuStatus;
  /**
   * Whether to make the icon visible.
   */
  showIcon?: boolean;
  /**
   * The icon to display.
   */
  icon?: IconType;
  /**
   * Whether the user needs to be authentiated to view this menu.
   */
  authenticated?: boolean;
  /**
   * Which claims are required to view this menu.
   */
  claims?: Claim | Array<Claim>;
  /**
   * Which roles are required to view this menu.
   */
  roles?: Role | Array<Role>;
}

/**
 * MenuGroup provides a UI component to display a menu button with supplied menu items.
 * @param param0 Component properties.
 * @returns MenuGroup component.
 */
export const MenuGroup: React.FC<IMenuGroupProps> = ({
  label,
  status,
  showIcon = true,
  icon,
  children,
  authenticated,
  claims,
  roles,
  ...rest
}) => {
  const keycloak = useKeycloakWrapper();
  const refMenu = React.useRef<HTMLDivElement>(null);
  const [direction, setDirection] = React.useState('dn');
  const Icon = icon;

  React.useEffect(() => {
    if (direction === 'check') setDirection(isInViewport(refMenu.current) ? 'dn' : 'up');
  }, [direction]);

  if (authenticated && !keycloak.authenticated) return null;
  if (!!claims && (!keycloak.authenticated || !keycloak.hasClaim(claims))) return null;
  if (!!roles && (!keycloak.authenticated || !keycloak.hasRole(roles))) return null;

  return (
    <HMenu>
      <styled.MenuGroup>
        <HMenu.Button>
          <div data-for="main-tooltip-right" data-tip={status === MenuStatus.narrow ? label : ''}>
            {showIcon && Icon && <Icon />}
            {status === MenuStatus.full && label}
          </div>
        </HMenu.Button>
        <HMenu.Items ref={refMenu} className={direction}>
          {({ open }) => <MenuItems setDirection={setDirection}>{children}</MenuItems>}
        </HMenu.Items>
      </styled.MenuGroup>
    </HMenu>
  );
};

interface IChildrenProps extends HTMLAttributes<HTMLElement> {
  setDirection: React.Dispatch<React.SetStateAction<string>>;
}

/**
 * MenuItems provides a way to safely change state once the children are loaded.
 * This will allow the parent element to determine if it is in the viewport and update style accordingly.
 * @param param0
 * @returns
 */
const MenuItems: React.FC<IChildrenProps> = ({ children, setDirection }) => {
  React.useEffect(() => {
    setDirection('check');
  }, [setDirection]);
  return <>{children}</>;
};

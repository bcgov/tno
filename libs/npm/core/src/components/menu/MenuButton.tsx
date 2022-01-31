import { Menu as HMenu } from '@headlessui/react';
import { LiHTMLAttributes } from 'react';
import { IconType } from 'react-icons/lib';

import { Claim, Role, useKeycloakWrapper } from '../../hooks';
import { MenuStatus } from '.';

interface IMenuButtonProps extends LiHTMLAttributes<HTMLLIElement> {
  /**
   * The text label of the button.
   */
  label: string;
  /**
   * Callback onClick event.
   */
  onClick?: () => void;
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
 * MenuButton provides a clickable button that will display a sub-menu or go to the route specified.
 * @param param0 Component properties.
 * @returns MenuButton component.
 */
export const MenuButton: React.FC<IMenuButtonProps> = ({
  label,
  onClick,
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
  const Icon = icon;

  if (authenticated && !keycloak.authenticated) return null;
  if (!!claims && (!keycloak.authenticated || !keycloak.hasClaim(claims))) return null;
  if (!!roles && (!keycloak.authenticated || !keycloak.hasRole(roles))) return null;

  return (
    <HMenu>
      <HMenu.Button>
        <div
          data-for="main-tooltip-right"
          data-tip={status === MenuStatus.narrow ? label : ''}
          onClick={() => !!onClick && onClick()}
        >
          {showIcon && Icon && <Icon />}
          {status === MenuStatus.full && label}
          {children}
        </div>
      </HMenu.Button>
    </HMenu>
  );
};

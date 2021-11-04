import { Menu as HMenu } from '@headlessui/react';
import React from 'react';
import { FaHome, FaToolbox } from 'react-icons/fa';
import { Link } from 'react-router-dom';
import { Menu, MenuButton, MenuContext, MenuGroup, MenuStatus } from 'tno-core';

interface IMenuProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Whether to display the icons.
   */
  showIcons?: boolean;
}

/**
 * Menu provides a navigation menu for the application.
 * @param param0 Component properties.
 * @returns Menu component.
 */
export const NavMenu: React.FC<IMenuProps> = ({ showIcons = true, children, ...rest }) => {
  const { status } = React.useContext(MenuContext);

  return status !== MenuStatus.hidden ? (
    <Menu {...rest}>
      <MenuButton label="Home" route="/" status={status} icon={FaHome}></MenuButton>
      <MenuGroup label="Administration" status={status} icon={FaToolbox}>
        <HMenu.Item as="div">{(props) => <Link to="/admin/users">Users</Link>}</HMenu.Item>
        <HMenu.Item as="div">{(props) => <Link to="/admin/kafka">Kafka Topics</Link>}</HMenu.Item>
      </MenuGroup>
      {children}
    </Menu>
  ) : null;
};

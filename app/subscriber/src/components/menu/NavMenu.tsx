import { Menu as HMenu } from '@headlessui/react';
import React from 'react';
import {
  FaBlackTie,
  FaBroadcastTower,
  FaCanadianMapleLeaf,
  FaExclamationCircle,
  FaFilter,
  FaHome,
  FaNewspaper,
  FaPodcast,
  FaScroll,
  FaSearch,
  FaStickyNote,
  FaTv,
  FaTwitter,
  FaUserTie,
  FaWifi,
} from 'react-icons/fa';
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
      <MenuButton label="Filters" route="/filters" status={status} icon={FaFilter}></MenuButton>

      <MenuButton
        label="My Quick Search"
        route="/search"
        status={status}
        showIcon={showIcons}
        icon={FaSearch}
      />
      <MenuButton
        label="My Minister"
        route="/my/minister"
        status={status}
        showIcon={showIcons}
        icon={FaUserTie}
      />
      <MenuButton
        label="Top Stories"
        route="/top/stories"
        status={status}
        showIcon={showIcons}
        icon={FaNewspaper}
      />
      <MenuButton
        label="Front Pages"
        route="/frontpage"
        status={status}
        showIcon={showIcons}
        icon={FaExclamationCircle}
      />
      <MenuButton
        label="Daily Print"
        route="/daily"
        status={status}
        showIcon={showIcons}
        icon={FaScroll}
      />
      <MenuButton
        label="Weekly Print"
        route="/weekly"
        status={status}
        showIcon={showIcons}
        icon={FaStickyNote}
      />
      <MenuButton
        label="Radio News"
        route="/radio"
        status={status}
        showIcon={showIcons}
        icon={FaBroadcastTower}
      />
      <MenuButton label="Television" route="/tv" status={status} showIcon={showIcons} icon={FaTv} />
      <MenuButton
        label="Internet"
        route="/internet"
        status={status}
        showIcon={showIcons}
        icon={FaWifi}
      />
      <MenuButton
        label="Social Media"
        route="/social"
        status={status}
        showIcon={showIcons}
        icon={FaTwitter}
      />
      <MenuButton
        label="CP News"
        route="/cpnews"
        status={status}
        showIcon={showIcons}
        icon={FaCanadianMapleLeaf}
      />
      <MenuButton label="S&S" route="/ss" status={status} showIcon={showIcons} icon={FaBlackTie} />
      <MenuButton
        label="Webcasts"
        route="/webcasts"
        status={status}
        showIcon={showIcons}
        icon={FaPodcast}
      />
      {children}
    </Menu>
  ) : null;
};

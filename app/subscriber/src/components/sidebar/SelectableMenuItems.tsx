import { MenuItemNames } from 'components/layout/constants/MenuItemNames';
import React from 'react';
import {
  FaClipboard,
  FaComment,
  FaFire,
  FaHeart,
  FaHome,
  FaSlidersH,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';
import { MenuItem } from 'react-pro-sidebar';
import { useNavigate } from 'react-router';

export interface ISelectableMenuItemProps {
  // array containing the names of the items to be placed in the sidebar
  menuItems: string[];
}

/**
 * SelectableMenuItems returns MenuItems for react-pro-sidebar. These menu items will be state controlled and display a
 * selection marker when selected. Simply provide an array of strings containing the name of the menu items to be.
 * @param menuItems Array of strings containing the names of the menu items to be displayed.
 * @returns SelectableMenuItems component.
 */
export const SelectableMenuItems: React.FC<ISelectableMenuItemProps> = ({ menuItems }) => {
  // state variable to tell if the menu item is selected
  const [selected, setSelected] = React.useState('Home');
  // controls route navigation
  const navigate = useNavigate();

  const getKeyByValue = (object: any, value: any) => {
    return Object.keys(object).find((key) => object[key] === value);
  };

  // when adding a new element for the menu first add the items name to MenuItemNames.ts and then add the icon here
  const determineIcon = (item: string): React.ReactElement => {
    switch (item) {
      case MenuItemNames.Home:
        return <FaHome />;
      case MenuItemNames.TopStories:
        return <FaFire />;
      case MenuItemNames.MyMinister:
        return <FaUserTie />;
      case MenuItemNames.TodaysCommentary:
        return <FaComment />;
      case MenuItemNames.PressGallery:
        return <FaUsers />;
      case MenuItemNames.FilterMedia:
        return <FaSlidersH />;
      case MenuItemNames.MyCollections:
        return <FaHeart />;
      case MenuItemNames.MyReports:
        return <FaClipboard />;
      default:
        return <FaHome />;
    }
  };
  return (
    <>
      {menuItems.map((item) => {
        console.log(item);
        return (
          <MenuItem
            onClick={() => {
              navigate(`landing/${getKeyByValue(MenuItemNames, item)}` ?? '');
              setSelected(item);
            }}
            className={selected === item ? 'selected' : ''}
            key={item}
            icon={determineIcon(item)}
          >
            {item}
          </MenuItem>
        );
      })}
    </>
  );
};

import { SidebarMenuItem } from 'components/layout/constants/SidebarMenuItems';
import React from 'react';
import { MenuItem } from 'react-pro-sidebar';
import { useNavigate } from 'react-router';

export interface ISelectableMenuItemProps {
  // array containing the names of the items to be placed in the sidebar
  menuItems: SidebarMenuItem[];
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

  return (
    <>
      {menuItems.map((item) => {
        return (
          <MenuItem
            onClick={() => {
              navigate(`landing/${item.path}` ?? '');
              setSelected(item.label);
            }}
            className={selected === item.label ? 'selected' : ''}
            key={item.label}
            icon={item.icon}
          >
            {item.label}
          </MenuItem>
        );
      })}
    </>
  );
};

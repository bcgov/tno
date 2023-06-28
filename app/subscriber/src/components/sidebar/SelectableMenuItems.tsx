import { sidebarMenuItemsArray } from 'components/layout/constants/SidebarMenuItems';
import React from 'react';
import { MenuItem } from 'react-pro-sidebar';
import { useNavigate } from 'react-router';
import { Row, Show } from 'tno-core';

export interface ISelectableMenuItemProps {}

/**
 * SelectableMenuItems returns MenuItems for react-pro-sidebar. These menu items will be state controlled and display a
 * selection marker when selected. Simply provide an array of strings containing the name of the menu items to be.
 * @param menuItems Array of strings containing the names of the menu items to be displayed.
 * @returns SelectableMenuItems component.
 */
export const SelectableMenuItems: React.FC<ISelectableMenuItemProps> = () => {
  // state variable to tell if the menu item is selected
  const [selected, setSelected] = React.useState('Home');
  // controls route navigation
  const navigate = useNavigate();

  return (
    <>
      {sidebarMenuItemsArray.map((item) => {
        return (
          <MenuItem
            className={selected === item.label ? 'selected' : ''}
            key={item.label}
            icon={item.icon}
            onClick={() => {
              navigate(`landing/${item.path}` ?? '');
              setSelected(item.label);
            }}
          >
            <Row className="label-container">
              <div>{item.label}</div>
              <Show visible={!!item.secondaryIcon}>
                <div
                  onClick={(e) => {
                    e.stopPropagation();
                    setSelected(item.label);
                    navigate(item.secondaryIconRoute ?? '');
                  }}
                  className="secondary-icon"
                >
                  {item.secondaryIcon}
                </div>
              </Show>
            </Row>
          </MenuItem>
        );
      })}
    </>
  );
};

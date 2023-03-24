import { MenuItemNames } from 'components/layout/constants/MenuItemNames';
import React from 'react';
import { FaAngleDoubleLeft, FaAngleDoubleRight } from 'react-icons/fa';
import { Menu, useProSidebar } from 'react-pro-sidebar';
import { Row, Show } from 'tno-core';

import { SelectableMenuItems } from './SelectableMenuItems';
import * as styled from './styled';

/**
 * CustomSidebar uses react-pro-sidebar to display a sidebar with a menu of selectable menu items. Incorporates the MMIA subscriber look and feel.
 * @returns Sidebar component.
 */
export const CustomSidebar: React.FC = () => {
  const { collapseSidebar, collapsed } = useProSidebar();
  return (
    <styled.CustomSidebar>
      <Menu>
        <Row className="title">
          <img
            alt="BC Gov logo"
            src={!collapsed ? '/assets/mminsights_logo.svg' : '/assets/mm_logo.svg'}
          />
        </Row>
        <SelectableMenuItems menuItems={Object.values(MenuItemNames)} />
      </Menu>
      <Show visible={!collapsed}>
        <FaAngleDoubleLeft className="collapse" onClick={() => collapseSidebar()} />
      </Show>
      <Show visible={collapsed}>
        <FaAngleDoubleRight className="collapse" onClick={() => collapseSidebar()} />
      </Show>
    </styled.CustomSidebar>
  );
};

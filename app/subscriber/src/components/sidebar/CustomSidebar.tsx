import React from 'react';
import { FaAngleDoubleLeft, FaAngleDoubleRight } from 'react-icons/fa';
import { Menu, useProSidebar } from 'react-pro-sidebar';
import { useNavigate } from 'react-router';
import { Row, Show } from 'tno-core';

import { SelectableMenuItems } from './SelectableMenuItems';
import * as styled from './styled';

export interface ICustomSidebarProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * CustomSidebar uses react-pro-sidebar to display a sidebar with a menu of selectable menu items. Incorporates the subscriber look and feel.
 * @returns Sidebar component.
 */
export const CustomSidebar: React.FC<ICustomSidebarProps> = () => {
  const { collapseSidebar, collapsed } = useProSidebar();
  const navigate = useNavigate();
  return (
    <styled.CustomSidebar>
      <Menu>
        <Row className="title" onClick={() => navigate('/')}>
          <img
            alt="BC Gov logo"
            src={!collapsed ? '/assets/mminsights_logo.svg' : '/assets/mm_logo.svg'}
          />
        </Row>
        <SelectableMenuItems />
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

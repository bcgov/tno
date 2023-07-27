import React from 'react';
import { FaAngleDoubleLeft, FaAngleDoubleRight } from 'react-icons/fa';
import { Menu, useProSidebar } from 'react-pro-sidebar';
import { Row, Show } from 'tno-core';

import { AdvancedSearch } from './advanced-search/AdvancedSearch';
import { SelectableMenuItems } from './SelectableMenuItems';
import * as styled from './styled';

export interface ICustomSidebarProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Whether advanced search is active or not
   * @default false
   */
  advancedSearch: boolean;
  /**
   * Event when advanced search is toggled.
   * @param expanded Whether advanced search is expanded or not.
   * @default false
   * @returns void
   */
  setAdvancedSearch: (expanded: boolean) => void;
}

/**
 * CustomSidebar uses react-pro-sidebar to display a sidebar with a menu of selectable menu items. Incorporates the MMIA subscriber look and feel.
 * @returns Sidebar component.
 */
export const CustomSidebar: React.FC<ICustomSidebarProps> = ({
  advancedSearch,
  setAdvancedSearch,
}) => {
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
        <Show visible={!collapsed}>
          <AdvancedSearch expanded={advancedSearch} setExpanded={setAdvancedSearch} />
        </Show>
        {!advancedSearch && <SelectableMenuItems />}
      </Menu>
      <Show visible={!collapsed && !advancedSearch}>
        <FaAngleDoubleLeft className="collapse" onClick={() => collapseSidebar()} />
      </Show>
      <Show visible={collapsed}>
        <FaAngleDoubleRight className="collapse" onClick={() => collapseSidebar()} />
      </Show>
    </styled.CustomSidebar>
  );
};

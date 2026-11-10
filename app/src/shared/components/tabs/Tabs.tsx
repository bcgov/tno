import React from 'react';

import { TabContainer, TabMenu } from '.';
import * as styled from './styled';

export interface ITabsProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Provide your <Tab/> elements.
   */
  tabs?: React.ReactNode;
}

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const Tabs: React.FC<ITabsProps> = ({ tabs, children, className }) => {
  return (
    <styled.Tabs className={`tabs${!!className ? ` ${className}` : ''}`}>
      {tabs && <TabMenu>{tabs}</TabMenu>}
      {tabs && <TabContainer>{children}</TabContainer>}
      {!tabs && children}
    </styled.Tabs>
  );
};

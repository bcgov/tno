import { Row } from 'components/row';
import React from 'react';

import * as styled from './styled';

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const TabMenu: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({
  children,
  className,
}) => {
  return (
    <styled.TabMenu className={`${className ?? 'tab-menu'}`}>
      <Row>{children}</Row>
    </styled.TabMenu>
  );
};

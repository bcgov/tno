import React from 'react';

import * as styled from './styled';

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const TabContainer: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({
  children,
  className,
}) => {
  return (
    <styled.TabContainer className={`tab-container${!!className ? ` ${className}` : ''}`}>
      {children}
    </styled.TabContainer>
  );
};

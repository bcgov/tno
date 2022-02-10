import React from 'react';

import * as styled from './NavBarStyled';

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const NavBarGroup: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ children }) => {
  return <styled.NavBarGroup>{children}</styled.NavBarGroup>;
};

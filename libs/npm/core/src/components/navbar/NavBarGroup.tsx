import React from 'react';

import * as styled from './styled';

export interface INavBarGroupProps extends React.HTMLAttributes<HTMLDivElement> {
  /** whether this part of the navbar should hover over the screen with a higher z-index */
  hover?: boolean;
}

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const NavBarGroup: React.FC<INavBarGroupProps> = ({ children, ...rest }) => {
  return <styled.NavBarGroup {...rest}>{children}</styled.NavBarGroup>;
};

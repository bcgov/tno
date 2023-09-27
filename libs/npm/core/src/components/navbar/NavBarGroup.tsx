import React from 'react';

import * as styled from './styled';

export interface INavBarGroupProps extends React.HTMLAttributes<HTMLDivElement> {
  /** whether this part of the navbar should hover over the screen with a higher z-index */
  hover?: boolean;
  /** Z-Index when hovering. */
  zIndex?: number;
}

/**
 * The element that groups the various navigation bar items together.
 * @param children the navigation bar items
 * @returns navigation bar group
 */
export const NavBarGroup: React.FC<INavBarGroupProps> = ({ children, zIndex = 100, ...rest }) => {
  return (
    <styled.NavBarGroup zIndex={zIndex} {...rest}>
      {children}
    </styled.NavBarGroup>
  );
};

import React from 'react';

import { MenuContext, MenuStatus } from '.';
import * as styled from './styled';

interface IMenuProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Whether to display the icons.
   */
  showIcons?: boolean;
}

/**
 * Menu provides a navigation menu for the application.
 * @param param0 Component properties.
 * @returns Menu component.
 */
export const Menu: React.FC<IMenuProps> = ({ showIcons = true, children, ...rest }) => {
  const { status } = React.useContext(MenuContext);
  return status !== MenuStatus.hidden ? <styled.Menu {...rest}>{children}</styled.Menu> : null;
};

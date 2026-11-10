import React from 'react';
import { IconBaseProps } from 'react-icons';

import * as styled from './styled';

/**
 * LogoutButton provides an icon button to enable the user to logout.
 * @param props Icon element attributes.
 * @returns LogoutButton component.
 */
export const LogoutButton: React.FC<IconBaseProps> = (props) => {
  return (
    <>
      <styled.LogoutButton
        data-tooltip-id="main-tooltip"
        data-tooltip-content="Logout"
        {...props}
      />
    </>
  );
};

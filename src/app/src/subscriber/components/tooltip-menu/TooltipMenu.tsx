import React from 'react';
import { ITooltip } from 'react-tooltip';

import * as styled from './styled';

/** Tooltip menu is a styled react-tooltip that adheres to the subscriber apps use of tooltip menus. Extends the default ITooltip interface
 * refer to https://www.npmjs.com/package/react-tooltip for more information on the react-tooltip package.
 */
export const TooltipMenu: React.FC<ITooltip> = ({ ...rest }) => {
  return (
    <styled.TooltipMenu noArrow variant="light" className="react-tooltip" {...rest}>
      {rest.children}
    </styled.TooltipMenu>
  );
};

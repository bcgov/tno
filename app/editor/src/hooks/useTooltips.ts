import React from 'react';
import ReactTooltip from 'react-tooltip';

/**
 * Tooltips require being rebuilt when a component is created or they will not be displayed.
 */
export const useTooltips = () => {
  React.useEffect(() => {
    ReactTooltip.rebuild();
  });
};

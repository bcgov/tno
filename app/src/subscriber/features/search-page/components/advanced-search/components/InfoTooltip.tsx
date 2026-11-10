import { TooltipMenu } from 'components/tooltip-menu';
import React from 'react';
import { ITooltip } from 'react-tooltip';

/**
 * An info popup for the advanced search panel. It is positioned against the viewport so the
 * panel's overflow cannot clip it, is fully opaque, sits above the page, and scrolls rather than
 * running off the screen.
 */
export const InfoTooltip: React.FC<ITooltip> = ({ style, ...rest }) => {
  return (
    <TooltipMenu
      openOnClick
      positionStrategy="fixed"
      opacity={1}
      style={{
        zIndex: 1100,
        maxWidth: 'calc(100vw - 1rem)',
        maxHeight: 'calc(100vh - 1rem)',
        overflow: 'auto',
        ...style,
      }}
      {...rest}
    />
  );
};

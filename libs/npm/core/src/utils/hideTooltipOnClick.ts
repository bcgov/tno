import ReactTooltip from 'react-tooltip';

type TooltipRef = { tooltipRef: null } | null;

/**
 * Function that takes in a tooltip ref and hides the tooltip when this function is called.
 * @param  {TooltipRef} tooltipRef React.useRef().current associated to the tooltip
 */
export const hideTooltipOnClick = (tooltipRef: TooltipRef) => {
  const current = tooltipRef;
  current!.tooltipRef = null;
  ReactTooltip.hide();
};

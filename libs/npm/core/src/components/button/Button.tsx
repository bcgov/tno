import React, { ButtonHTMLAttributes } from 'react';
import ReactTooltip from 'react-tooltip';

import { hideTooltipOnClick } from '../../utils';
import { BouncingSpinner } from '../spinners';
import { ButtonVariant } from '.';
import * as styled from './styled';

export interface IButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * The styled variant.
   */
  variant?: ButtonVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Whether the button should show a loading indicator.
   */
  loading?: boolean;
}

/**
 * Button component provides a bootstrapped styled button element.
 * @param param0 Button element attributes.
 * @returns Button component.
 */
export const Button: React.FC<IButtonProps> = ({
  type = 'button',
  variant = ButtonVariant.primary,
  tooltip,
  className,
  children,
  loading = false,
  ...rest
}) => {
  const tip = React.useRef(null);
  const onClickHandler = (event: React.MouseEvent<HTMLButtonElement>) => {
    tooltip && hideTooltipOnClick(tip.current);
    rest.onClick && rest.onClick(event);
  };
  return (
    <styled.Button
      type={type}
      variant={variant}
      onClick={onClickHandler}
      className={`btn ${className ?? ''}`}
      data-for="button-tooltip"
      data-tip={tooltip}
      {...rest}
    >
      <div>
        {children}
        {loading && <BouncingSpinner />}
      </div>
      <ReactTooltip ref={tip} id="button-tooltip" effect="float" type="light" place="top" />
    </styled.Button>
  );
};

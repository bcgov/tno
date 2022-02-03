import React, { ButtonHTMLAttributes } from 'react';

import { ButtonVariant } from '.';
import * as styled from './ButtonStyled';

export interface IButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  /**
   * The styled variant.
   */
  variant?: ButtonVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
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
  ...rest
}) => {
  return (
    <styled.Button
      type={type}
      variant={variant}
      className={`btn ${className ?? ''}`}
      data-for="main-tooltip"
      data-tip={tooltip}
      {...rest}
    >
      {children}
    </styled.Button>
  );
};

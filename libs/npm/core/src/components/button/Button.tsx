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
  variant = ButtonVariant.primary,
  tooltip,
  children,
  ...rest
}) => {
  return (
    <styled.Button
      type="button"
      variant={variant}
      {...rest}
      className="btn"
      data-for="main"
      data-tip={tooltip}
    >
      {children}
    </styled.Button>
  );
};

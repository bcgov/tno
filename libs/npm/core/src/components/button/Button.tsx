import React, { ButtonHTMLAttributes } from 'react';

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
  return (
    <styled.Button
      type={type}
      variant={variant}
      className={`btn ${className ?? ''}`}
      data-for="main-tooltip"
      data-tip={tooltip}
      {...rest}
    >
      <div>
        {children}
        {loading && <BouncingSpinner />}
      </div>
    </styled.Button>
  );
};

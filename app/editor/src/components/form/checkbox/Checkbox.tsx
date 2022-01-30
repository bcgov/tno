import React, { InputHTMLAttributes } from 'react';

import { CheckboxVariant } from '.';
import * as styled from './CheckboxStyled';

export interface ICheckboxProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * The styled variant.
   */
  variant?: CheckboxVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

/**
 * Checkbox component provides a bootstrapped styled button element.
 * @param param0 Checkbox element attributes.
 * @returns Checkbox component.
 */
export const Checkbox: React.FC<ICheckboxProps> = ({
  type = 'checkbox',
  variant = CheckboxVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <styled.Checkbox
      type={type}
      variant={variant}
      className={`${className}`}
      data-for="main"
      data-tip={tooltip}
      {...rest}
    >
      {children}
    </styled.Checkbox>
  );
};

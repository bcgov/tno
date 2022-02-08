import React, { InputHTMLAttributes } from 'react';

import { CheckboxVariant } from '.';
import * as styled from './CheckboxStyled';

export interface ICheckboxProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: CheckboxVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
  /**
   * Reference to DOM.
   */
  ref?: any;
}

/**
 * Checkbox component provides a bootstrapped styled input checkbox element.
 * @param param0 Checkbox element attributes.
 * @returns Checkbox component.
 */
export const Checkbox: React.FC<ICheckboxProps> = ({
  name,
  label,
  value,
  type = 'checkbox',
  variant = CheckboxVariant.primary,
  tooltip,
  children,
  ref,
  ...rest
}) => {
  return (
    <div className="frm-in chk" data-for="main-tooltip" data-tip={tooltip}>
      <styled.Checkbox
        id={`${name}-${value}`}
        name={name}
        value={value}
        ref={ref}
        type={type}
        variant={variant}
        {...rest}
      >
        {children}
      </styled.Checkbox>
      <label htmlFor={`${name}-${value}`}>{label}</label>
    </div>
  );
};

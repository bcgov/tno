import React from 'react';
import { Props } from 'react-select';
import Select from 'react-select';

import { DropdownVariant } from '.';
import * as styled from './DropdownStyled';

export interface IDropdownProps {
  /**
   * The label to include with the control.
   */
  label?: string;
  /**
   * The styled variant.
   */
  variant?: DropdownVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

export interface ISelectProps extends IDropdownProps, Props {}

/**
 * Dropdown component provides a bootstrapped styled button element.
 * @param param0 Dropdown element attributes.
 * @returns Dropdown component.
 */
export const Dropdown: React.FC<ISelectProps> = ({
  name,
  label,
  variant = DropdownVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <styled.Dropdown className="frm-in" variant={variant}>
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      <Select
        // variant={variant}
        name={name}
        className={`slt ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        {...rest}
      />
    </styled.Dropdown>
  );
};

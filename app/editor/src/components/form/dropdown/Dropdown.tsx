import React, { SelectHTMLAttributes } from 'react';

import { DropdownVariant, instanceOfIOption, IOption } from '.';
import * as styled from './DropdownStyled';

export interface IDropdownProps extends SelectHTMLAttributes<HTMLSelectElement> {
  /**
   * The control name.
   */
  name: string;
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
  /**
   * An array of options.
   */
  options?: readonly string[] | number[] | IOption[];
}

/**
 * Dropdown component provides a bootstrapped styled button element.
 * @param param0 Dropdown element attributes.
 * @returns Dropdown component.
 */
export const Dropdown: React.FC<IDropdownProps> = ({
  name,
  label,
  variant = DropdownVariant.primary,
  tooltip,
  children,
  className,
  options,
  ...rest
}) => {
  return (
    <>
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      <styled.Dropdown
        variant={variant}
        name={name}
        {...rest}
        className={`${className}`}
        data-for="main"
        data-tip={tooltip}
      >
        {options
          ? options.map((option) => {
              if (instanceOfIOption(option)) {
                const item = option as IOption;
                return (
                  <option key={item.value} value={item.value}>
                    {item.label}
                  </option>
                );
              } else {
                const value = option as string;
                return (
                  <option key={value} value={value}>
                    {option}
                  </option>
                );
              }
            })
          : children}
      </styled.Dropdown>
    </>
  );
};

import React, { InputHTMLAttributes } from 'react';

import { instanceOfIOption, IOptionItem } from '..';
import { Checkbox, CheckboxVariant } from '.';

export interface ICheckboxGroupProps extends InputHTMLAttributes<HTMLInputElement> {
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
  variant?: CheckboxVariant;
  /**
   * An array of options.
   */
  options?: readonly string[] | number[] | IOptionItem[] | HTMLOptionElement[];
}

/**
 * CheckboxGroup component provides a bootstrapped styled input checkbox element.
 * @param param0 CheckboxGroup element attributes.
 * @returns CheckboxGroup component.
 */
export const CheckboxGroup: React.FC<ICheckboxGroupProps> = ({
  name,
  label,
  children,
  options,
  ...rest
}) => {
  return (
    <div className="frm-in chk">
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      {options
        ? options.map((option) => {
            if (instanceOfIOption(option)) {
              const item = option as IOptionItem;
              return (
                <span key={item.value}>
                  <Checkbox id={`${name}-${item.value}`} name={name} value={item.value} {...rest} />
                  <label htmlFor={`${name}-${item.value}`}>{item.label}</label>
                </span>
              );
            } else if (typeof option === 'object') {
              // TODO: Validate option is HTMLOptionElement
              return option;
            } else {
              const value = option as string;
              return (
                <span key={value}>
                  <Checkbox id={`${name}-${value}`} name={name} value={value} {...rest} />
                  <label htmlFor={`${name}-${value}`}>{value}</label>
                </span>
              );
            }
          })
        : children}
    </div>
  );
};

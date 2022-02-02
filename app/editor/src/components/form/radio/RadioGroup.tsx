import React, { InputHTMLAttributes } from 'react';

import { instanceOfIOption, IOptionItem } from '..';
import { Radio, RadioVariant } from '.';

export interface IRadioGroupProps extends InputHTMLAttributes<HTMLInputElement> {
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
  variant?: RadioVariant;
  /**
   * An array of options.
   */
  options?: readonly string[] | number[] | IOptionItem[] | HTMLOptionElement[];
}

/**
 * RadioGroup component provides a bootstrapped styled input radio element.
 * @param param0 RadioGroup element attributes.
 * @returns RadioGroup component.
 */
export const RadioGroup: React.FC<IRadioGroupProps> = ({
  name,
  label,
  children,
  options,
  ...rest
}) => {
  return (
    <div className="frm-in rad">
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      {options
        ? options.map((option) => {
            if (instanceOfIOption(option)) {
              const item = option as IOptionItem;
              return (
                <span key={item.value}>
                  <Radio id={`${name}-${item.value}`} name={name} value={item.value} {...rest} />
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
                  <Radio id={`${name}-${value}`} name={name} value={value} {...rest} />
                  <label htmlFor={`${name}-${value}`}>{value}</label>
                </span>
              );
            }
          })
        : children}
    </div>
  );
};

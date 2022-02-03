import React, { InputHTMLAttributes } from 'react';

import { instanceOfIOption, IOptionItem } from '..';
import { Radio, RadioVariant } from '.';

export interface IRadioGroupProps<OT extends string | number | IOptionItem | HTMLOptionElement>
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'value' | 'onChange'> {
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
  options?: readonly OT[];
  /**
   * The current value.
   */
  value?: OT;
  /**
   * OnChange event
   */
  onChange?: (event: React.ChangeEvent<HTMLInputElement>, values: OT | undefined) => void;
}

/**
 * RadioGroup component provides a bootstrapped styled input radio element.
 * @param param0 RadioGroup element attributes.
 * @returns RadioGroup component.
 */
export const RadioGroup = <OT extends string | number | IOptionItem | HTMLOptionElement>({
  name,
  label,
  children,
  options,
  value,
  onChange,
  ...rest
}: IRadioGroupProps<OT>) => {
  const [selected, setSelected] = React.useState<OT | undefined>(value);

  const handleChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = e.target.value;
    const isChecked = e.target.checked;

    if (isChecked) {
      const found: OT = (options as any)?.find((option: any) => {
        if (typeof option === 'string') return option === value;
        if (typeof option === 'number') return option === +value;
        if (instanceOfIOption(option)) return `${option.value}` === value;
        return option === value;
      });
      if (found) setSelected(found);
      onChange?.(e, found);
    } else {
      setSelected(undefined);
      onChange?.(e, undefined);
    }
  };

  return (
    <div className="frm-in rad">
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      {options
        ? options.map((option) => {
            if (instanceOfIOption(option)) {
              const item = option as IOptionItem;
              return (
                <span key={item.value}>
                  <Radio
                    id={`${name}-${item.value}`}
                    name={name}
                    value={item.value}
                    checked={item.value === (selected as IOptionItem)?.value}
                    onChange={handleChange}
                    {...rest}
                  />
                  <label htmlFor={`${name}-${item.value}`}>{item.label}</label>
                </span>
              );
            } else if (typeof option === 'object') {
              // TODO: Validate option is HTMLOptionElement
              return option;
            } else if (typeof option === 'number') {
              const value = option as number;
              return (
                <span key={value}>
                  <Radio
                    id={`${name}-${value}`}
                    name={name}
                    value={value}
                    checked={value === selected}
                    {...rest}
                    onChange={handleChange}
                  />
                  <label htmlFor={`${name}-${value}`}>{value}</label>
                </span>
              );
            } else {
              const value = option as string;
              return (
                <span key={value}>
                  <Radio
                    id={`${name}-${value}`}
                    name={name}
                    value={value}
                    checked={value === selected}
                    {...rest}
                    onChange={handleChange}
                  />
                  <label htmlFor={`${name}-${value}`}>{value}</label>
                </span>
              );
            }
          })
        : children}
    </div>
  );
};

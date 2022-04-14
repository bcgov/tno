import React, { InputHTMLAttributes } from 'react';

import { instanceOfIOption, IOptionItem } from '..';
import { Checkbox, CheckboxVariant } from '.';

export interface ICheckboxGroupProps<CT extends string | number | IOptionItem | HTMLOptionElement>
  extends Omit<InputHTMLAttributes<HTMLInputElement>, 'onChange'> {
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
  options?: readonly CT[];
  /**
   * OnChange event
   */
  onChange?: (event: React.ChangeEvent<HTMLInputElement>, values: readonly CT[]) => void;
}

/**
 * CheckboxGroup component provides a bootstrapped styled input checkbox element.
 * @param param0 CheckboxGroup element attributes.
 * @returns CheckboxGroup component.
 */
export const CheckboxGroup = <CT extends string | number | IOptionItem | HTMLOptionElement>({
  name,
  label,
  children,
  options,
  onChange,
  ...rest
}: ICheckboxGroupProps<CT>) => {
  const [values, setValues] = React.useState<CT[]>([]);

  const handleChange: React.ChangeEventHandler<HTMLInputElement> = (e) => {
    const value = e.target.value;
    const isChecked = e.target.checked;
    let results = [...values];

    const found: CT = (options as any)?.find(
      (option: any) => option === value || (option as IOptionItem).value === value,
    );
    if (isChecked && found) {
      results.push(found);
    } else if (found) {
      results = results.filter((value) => value !== found);
    }

    setValues(results);

    onChange?.(e, results);
  };

  return (
    <div className="frm-in chg">
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      {options
        ? options.map((option, i) => {
            if (instanceOfIOption(option)) {
              const item = option as IOptionItem;
              return (
                <Checkbox
                  key={item.value}
                  id={`${name}-${item.value}`}
                  name={name}
                  label={item.label}
                  value={item.value}
                  onChange={handleChange}
                  {...rest}
                />
              );
            } else if (typeof option === 'object') {
              // TODO: Validate option is HTMLOptionElement
              const element = option as unknown as React.ReactNode;
              return element;
            } else {
              const value = option as string;
              return (
                <Checkbox
                  key={value}
                  id={`${name}-${value}`}
                  name={name}
                  label={value}
                  value={value}
                  {...rest}
                />
              );
            }
          })
        : children}
    </div>
  );
};

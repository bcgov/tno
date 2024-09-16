import React from 'react';

import { instanceOfIOption, IOptionItem } from '..';
import { ICheckboxGroupProps, SmallCheckbox } from '.';

/**
 * CheckboxGroup component provides a bootstrapped styled input checkbox element.
 * @param param0 CheckboxGroup element attributes.
 * @returns CheckboxGroup component.
 */
export const SmallCheckboxGroup = <CT extends string | number | IOptionItem | HTMLOptionElement>({
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
                <SmallCheckbox
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
                <SmallCheckbox
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

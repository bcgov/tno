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
  const [selected, setSelected] = React.useState<any | readonly any[] | undefined>();
  const [selectedValue, setSelectedValue] = React.useState<any>();

  React.useEffect(() => {
    if (options && options.length) {
      const results = (options as any[]).filter((option: any) => {
        // TODO: Handle other types.
        return instanceOfIOption(option) && (option as IOptionItem).selected;
      });
      if (results.length) {
        const values = rest.isMulti ? results : results[results.length - 1];
        setSelected(values);
        setSelectedValue(values);
      }
    }
  }, [options, rest.isMulti]);

  return (
    <styled.Dropdown className="frm-in" variant={variant}>
      {label && <label htmlFor={`dpn-${name}`}>{label}</label>}
      <Select
        // variant={variant}
        name={name}
        className={`slt ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        value={value ?? selectedValue}
        defaultValue={defaultValue ?? selected}
        onChange={(newValue, actionMeta) => {
          setSelectedValue(newValue);
          onChange?.(newValue, actionMeta);
        }}
        options={options}
        {...rest}
      />
    </styled.Dropdown>
  );
};

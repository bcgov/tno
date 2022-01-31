import 'react-datepicker/dist/react-datepicker.css';

import React from 'react';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';

import { SelectDateVariant } from '.';
import * as styled from './SelectDateStyled';

export interface ISelectDateProps {
  label?: string;
  /**
   * The styled variant.
   */
  variant?: SelectDateVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

export interface IDatePickerProps extends ISelectDateProps, ReactDatePickerProps {}

/**
 * SelectDate component provides a bootstrapped styled button element.
 * @param param0 SelectDate element attributes.
 * @returns SelectDate component.
 */
export const SelectDate: React.FC<IDatePickerProps> = ({
  id,
  name,
  label,
  variant = SelectDateVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <styled.SelectDate className="frm-in">
      {label && <label htmlFor={id ?? `txt-${name}`}>{label}</label>}
      <DatePicker
        name={name}
        id={id}
        className={`dpk ${className ?? ''}`}
        data-for="main-tooltip"
        data-tip={tooltip}
        {...rest}
      />
    </styled.SelectDate>
  );
};

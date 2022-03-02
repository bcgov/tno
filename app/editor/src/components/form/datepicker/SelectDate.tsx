import 'react-datepicker/dist/react-datepicker.css';

import React, { useState } from 'react';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';

import { SelectDateVariant } from '.';
import * as styled from './styled';

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
  /**
   * Whether an initial date is provided for the datepicker, will set the selected value of the
   * input if provided.
   */
  selectedDate?: string;
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
  selectedDate,
  ...rest
}) => {
  const [startDate, setStartDate] = useState(selectedDate ? new Date(selectedDate) : new Date());

  return (
    <styled.SelectDate className="frm-in">
      {label && <label htmlFor={id ?? `txt-${name}`}>{label}</label>}
      {selectedDate ? (
        <DatePicker
          name={name}
          id={id}
          selected={startDate}
          onChange={(date: any) => setStartDate(date)}
          className={`dpk ${className ?? ''}`}
          data-for="main-tooltip"
          data-tip={tooltip}
          disabled={rest.disabled}
        />
      ) : (
        <DatePicker
          name={name}
          id={id}
          className={`dpk ${className ?? ''}`}
          data-for="main-tooltip"
          data-tip={tooltip}
          {...rest}
        />
      )}
    </styled.SelectDate>
  );
};

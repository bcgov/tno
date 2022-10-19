import 'react-datepicker/dist/react-datepicker.css';

import { Error } from 'components/form';
import React, { useState } from 'react';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';
import { FieldSize } from 'tno-core';

import { SelectDateVariant } from '.';
import * as styled from './styled';

export interface ISelectDateProps {
  /**
   * The label to display with the input.
   */
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
  /** Whether the form field is required. */
  required?: boolean;
  /** The form field size. */
  width?: FieldSize | string;
  /** Error related to the date picker */
  error?: string;
}

export interface IDatePickerProps extends ISelectDateProps, ReactDatePickerProps {}

/**
 * SelectDate component provides a bootstrapped styled button element.
 * @param param0 SelectDate element attributes.
 * @returns SelectDate component.
 */
export const SelectDate: React.FC<IDatePickerProps> = ({
  id,
  name = 'date',
  label,
  variant = SelectDateVariant.primary,
  tooltip,
  children,
  className,
  selectedDate,
  required,
  error,
  width = FieldSize.Stretch,
  onChange,
  ...rest
}) => {
  const [startDate, setStartDate] = useState(selectedDate ? new Date(selectedDate) : new Date());

  return (
    <styled.SelectDate
      className="frm-in"
      variant={!!error ? SelectDateVariant.warning : variant}
      required={required}
      error={error}
      width={width}
      role={!!error ? 'alert' : 'none'}
    >
      {label && (
        <label className={required ? 'required' : ''} htmlFor={id ?? `txt-${name}`}>
          {label}
        </label>
      )}
      {selectedDate ? (
        <>
          <DatePicker
            name={name}
            id={id}
            selected={startDate}
            onChange={(date: Date | null, event: React.SyntheticEvent<any, Event> | undefined) => {
              setStartDate(date as Date);
              if (onChange) onChange(date, event);
            }}
            className={`dpk${className ? ` ${className}` : ''}`}
            data-for="main-tooltip"
            data-tip={tooltip}
            disabled={rest.disabled}
            required={required}
            {...rest}
          />
          <Error error={error} />
        </>
      ) : (
        <>
          <DatePicker
            name={name}
            id={id}
            className={`dpk${className ? ` ${className}` : ''}`}
            data-for="main-tooltip"
            data-tip={tooltip}
            disabled={rest.disabled}
            required={required}
            onChange={onChange}
            {...rest}
          />
          <Error error={error} />
        </>
      )}
    </styled.SelectDate>
  );
};

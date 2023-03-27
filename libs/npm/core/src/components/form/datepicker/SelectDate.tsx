import 'react-datepicker/dist/react-datepicker.css';

import React from 'react';
import DatePicker, { ReactDatePickerProps } from 'react-datepicker';

import { Error, FieldSize } from '../../form';
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
  const [startDate, setStartDate] = React.useState(
    selectedDate ? new Date(selectedDate) : undefined,
  );

  React.useEffect(() => {
    const selected = selectedDate ? new Date(selectedDate) : undefined;
    if (selected && selected.getTime() !== startDate?.getTime()) {
      setStartDate(selected);
    }
  }, [selectedDate, startDate]);

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
      <DatePicker
        name={name}
        id={id}
        selected={startDate}
        onChange={(date: Date | null, event: React.SyntheticEvent<any, Event> | undefined) => {
          setStartDate(date as Date);
          if (onChange) onChange(date, event);
        }}
        className={`dpk${className ? ` ${className}` : ''}`}
        data-tooltip-id="main-tooltip"
        data-tooltip-content={tooltip}
        disabled={rest.disabled}
        required={required}
        {...rest}
      />
      <Error error={error} />
    </styled.SelectDate>
  );
};

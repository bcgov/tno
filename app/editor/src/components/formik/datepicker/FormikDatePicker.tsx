import { IDatePickerProps, SelectDate, SelectDateVariant } from 'components/form/datepicker';
import { getIn, useFormikContext } from 'formik';
import { FieldSize } from 'tno-core';

/**
 * Formik wrapped date picker.
 * @returns Formik wrapped SelectDate component.
 */
export const FormikDatePicker: React.FC<IDatePickerProps> = ({
  id,
  name = 'date',
  label,
  variant = SelectDateVariant.primary,
  tooltip,
  children,
  className,
  selectedDate,
  required,
  width = FieldSize.Stretch,
  onChange,
  ...rest
}) => {
  const { errors, touched } = useFormikContext<any>();

  const errorMessage = getIn(errors, name);
  const error = errorMessage && getIn(touched, name) && errorMessage;

  return (
    <SelectDate
      id={id}
      name={name}
      label={label}
      variant={variant}
      tooltip={tooltip}
      children={children}
      className={error ? `${className ?? ''} error` : className}
      selectedDate={selectedDate}
      required={required}
      width={width}
      error={error}
      onChange={onChange}
      {...rest}
    />
  );
};

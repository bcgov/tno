import { FieldSize } from 'components/form/constants';
import { IDatePickerProps, SelectDate, SelectDateVariant } from 'components/form/datepicker';
import { useFormikContext } from 'formik';

/**
 * Formik wrapped date picker.
 * @returns Formik wrapped SelectDate component.
 */
export const FormikDatePicker: React.FC<IDatePickerProps> = ({
  id,
  name,
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
  const error =
    (errors as any)[name ?? ''] && (touched as any)[name ?? ''] && (errors as any)[name ?? ''];

  return (
    <SelectDate
      id={id}
      name={name}
      label={label}
      variant={variant}
      tooltip={tooltip}
      children={children}
      className={className}
      selectedDate={selectedDate}
      required={required}
      width={width}
      error={error}
      onChange={onChange}
      {...rest}
    />
  );
};

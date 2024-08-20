import { getIn, useFormikContext } from 'formik';

import { ITimeInputProps, TimeInput } from '../../form/timeinput';

export interface IFormikTimeInputProps extends ITimeInputProps {
  name: string;
}

/**
 * FormikTimeInput component provides a formik controlled time input.
 * If you override `onBlur` or `onChange` you will need to possibly handle the formik integration.
 * @param param0 Component properties.
 * @returns
 */
export const FormikTimeInput = <T,>({
  id,
  name,
  value,
  disabled,
  onChange,
  onBlur,
  ...rest
}: IFormikTimeInputProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();

  const errorMessage = getIn(errors, name);
  const error = errorMessage && getIn(touched, name) && errorMessage;
  const fieldValue = getIn(values, name);

  return (
    <TimeInput
      name={name}
      error={error}
      value={value ?? fieldValue ?? ''}
      onChange={onChange ?? handleChange}
      onBlur={onBlur ?? handleBlur}
      disabled={disabled || isSubmitting}
      {...rest}
    />
  );
};

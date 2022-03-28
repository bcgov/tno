import { Checkbox, ICheckboxProps } from 'components/form';
import { getIn, useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikCheckboxProps extends ICheckboxProps {
  /** Name to identify form field, and also default pathname to field if 'field' is not provided. */
  name: string;
  /** Full name and path to form value.  Uses 'name' if you don't set this property. */
  field?: string;
}

export const FormikCheckbox = <T,>({
  name,
  field,
  value = true,
  onChange,
  onBlur,
  className,
  disabled,
  checked,
  ...rest
}: IFormikCheckboxProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();

  field = field ?? name;
  const error = (errors as any)[field] && (touched as any)[field] && (errors as any)[field];
  const fieldValue = getIn(values, field);

  return (
    <styled.FormikCheckbox>
      <Checkbox
        name={name}
        value={value ?? fieldValue ?? ''}
        checked={checked || fieldValue === value}
        onChange={(e) => {
          handleChange(e);
          if (onChange) onChange(e);
        }}
        onBlur={(e) => {
          handleBlur(e);
          if (onBlur) onBlur(e);
        }}
        className={error ? `${className ?? ''} error` : className}
        disabled={disabled || isSubmitting}
        error={error}
        {...rest}
      />
    </styled.FormikCheckbox>
  );
};

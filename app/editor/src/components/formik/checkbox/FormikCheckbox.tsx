import { Checkbox, ICheckboxProps } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikCheckboxProps extends ICheckboxProps {
  name: string;
  label?: string;
  value?: string | number | readonly string[];
  checked?: boolean;
}

export const FormikCheckbox = <T,>({
  name,
  value,
  onChange,
  onBlur,
  className,
  disabled,
  ...rest
}: IFormikCheckboxProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];

  return (
    <styled.FormikCheckbox>
      <Checkbox
        name={name}
        value={value ?? (values as any)[name] ?? ''}
        checked={!!(values as any)[name]}
        onChange={onChange ?? handleChange}
        onBlur={onBlur ?? handleBlur}
        className={error ? `${className ?? ''} error` : className}
        disabled={disabled || isSubmitting}
        error={error}
        {...rest}
      />
    </styled.FormikCheckbox>
  );
};

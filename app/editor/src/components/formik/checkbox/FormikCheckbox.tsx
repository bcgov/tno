import { Checkbox, ICheckboxProps } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikCheckboxProps extends ICheckboxProps {
  name: string;
  label?: string;
  value?: string | number | readonly string[];
  checked?: boolean;
  labelRight?: boolean;
}

export const FormikCheckbox = <T,>({
  id,
  name,
  label,
  value,
  onChange,
  onBlur,
  checked,
  className,
  disabled,
  labelRight,
  ...rest
}: IFormikCheckboxProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  return (
    <styled.FormikCheckbox>
      {label && !labelRight && <label htmlFor={id ?? `cbx-${name}`}>{label}</label>}
      <div className={labelRight ? 'rightHandLabel' : 'default'}>
        <Checkbox
          id={id ?? `cbx-${name}`}
          name={name}
          value={value ?? (values as any)[name] ?? ''}
          checked={checked}
          onChange={onChange ?? handleChange}
          onBlur={onBlur ?? handleBlur}
          className={error ? `${className ?? ''} error` : className}
          disabled={disabled || isSubmitting}
          {...rest}
        ></Checkbox>
        {error ? <p role="alert">{error}</p> : null}
        {labelRight && <label htmlFor={id ?? `cbx-${name}`}>{label}</label>}
      </div>
    </styled.FormikCheckbox>
  );
};

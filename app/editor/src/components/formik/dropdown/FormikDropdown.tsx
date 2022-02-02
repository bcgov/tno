import { Dropdown, IDropdownProps } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './FormikDropdownStyled';

export interface IFormikDropdownProps extends IDropdownProps {
  value?: string | number | readonly string[];
}

export const FormikDropdown = <T,>({
  id,
  name,
  label,
  value,
  children,
  className,
  disabled,
  onChange,
  onBlur,
  ...rest
}: IFormikDropdownProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  return (
    <styled.FormikDropdown>
      <div>
        <Dropdown
          id={id ?? `dpn-${name}`}
          name={name}
          value={value ?? (values as any)[name] ?? ''}
          onChange={onChange ?? handleChange}
          onBlur={onBlur ?? handleBlur}
          className={error ? `${className ?? ''} error` : className}
          disabled={disabled || isSubmitting}
          {...rest}
        >
          {children}
        </Dropdown>
        {error ? <p role="alert">{error}</p> : null}
      </div>
    </styled.FormikDropdown>
  );
};

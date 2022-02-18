import { IOptionItem } from 'components';
import { Dropdown, ISelectProps } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './FormikDropdownStyled';

export interface IFormikDropdownProps extends ISelectProps {
  name: string;
  value?: string | number | readonly string[] | IOptionItem;
  outerClassName?: string;
}

export const FormikDropdown = <T,>({
  id,
  name,
  label,
  value,
  className,
  isDisabled,
  onChange,
  outerClassName,
  onBlur,
  ...rest
}: IFormikDropdownProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  return (
    <styled.FormikDropdown>
      <div className={outerClassName}>
        {label && (
          <b>
            <label htmlFor={id ?? `txt-${name}`}>{label}</label>
          </b>
        )}
        <Dropdown
          id={id ?? `dpn-${name}`}
          name={name}
          value={value ?? (values as any)[name] ?? ''}
          onChange={onChange ?? handleChange}
          onBlur={onBlur ?? handleBlur}
          className={error ? `${className ?? ''} error` : className}
          isDisabled={isDisabled || isSubmitting}
          {...rest}
        ></Dropdown>
        {error ? <p role="alert">{error}</p> : null}
      </div>
    </styled.FormikDropdown>
  );
};

import { TextVariant } from 'components';
import { ITextProps, Text } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './FormikTextStyled';

export interface IFormikTextProps extends ITextProps {
  value?: string | number | readonly string[];
}

export const FormikText = <T,>({
  id,
  name,
  value,
  className,
  disabled,
  onChange,
  onBlur,
  ...rest
}: IFormikTextProps) => {
  const { values, errors, touched, handleBlur, handleChange, isSubmitting } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  return (
    <styled.FormikText>
      <Text
        id={id ?? `txt-${name}`}
        name={name}
        value={value ?? (values as any)[name] ?? ''}
        onChange={onChange ?? handleChange}
        onBlur={onBlur ?? handleBlur}
        className={error ? `${className ?? ''} error` : className}
        disabled={disabled || isSubmitting}
        variant={disabled ? TextVariant.disabled : TextVariant.primary}
        {...rest}
      ></Text>
      {error ? <p role="alert">{error}</p> : null}
    </styled.FormikText>
  );
};

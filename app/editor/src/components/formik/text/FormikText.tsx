import { ITextProps, Text, TextVariant } from 'components/form';
import { getIn, useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikTextProps extends ITextProps {}

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
  const fieldValue = getIn(values, name);

  return (
    <styled.FormikText>
      <Text
        id={id ?? `txt-${name}`}
        name={name}
        error={error}
        value={value ?? fieldValue ?? ''}
        onChange={onChange ?? handleChange}
        onBlur={onBlur ?? handleBlur}
        className={error ? `${className ?? ''} error` : className}
        disabled={disabled || isSubmitting}
        variant={disabled ? TextVariant.disabled : TextVariant.primary}
        {...rest}
      ></Text>
    </styled.FormikText>
  );
};

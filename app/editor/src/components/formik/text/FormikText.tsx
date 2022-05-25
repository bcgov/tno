import { ITextProps, Text, TextVariant } from 'components/form';
import { getIn, useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikTextProps extends ITextProps {}

/**
 * FormikText component provides a formik controlled text input.
 * If you override `onBlur` or `onChange` you will need to possibly handle the formik integration.
 * @param param0 Component properties.
 * @returns
 */
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

  const errorMessage = getIn(errors, name);
  const error = errorMessage && getIn(touched, name) && errorMessage;
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

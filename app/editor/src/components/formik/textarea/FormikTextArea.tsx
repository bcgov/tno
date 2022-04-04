import { ITextAreaProps, TextArea } from 'components/form';
import { getIn, useFormikContext } from 'formik';

import * as styled from './styled';

export interface IFormikTextAreaProps extends ITextAreaProps {
  value?: string | number | readonly string[];
}

export const FormikTextArea = <T,>({
  id,
  name,
  value,
  className,
  onChange,
  onBlur,
  ...rest
}: IFormikTextAreaProps) => {
  const { values, errors, touched, handleBlur, handleChange } = useFormikContext<T>();

  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  const fieldValue = getIn(values, name);

  return (
    <styled.FormikTextArea>
      <TextArea
        id={id ?? `txa-${name}`}
        name={name}
        error={error}
        value={value ?? fieldValue ?? ''}
        onChange={onChange ?? handleChange}
        onBlur={onBlur ?? handleBlur}
        className={error ? `${className ?? ''} error` : className}
        {...rest}
      ></TextArea>
    </styled.FormikTextArea>
  );
};

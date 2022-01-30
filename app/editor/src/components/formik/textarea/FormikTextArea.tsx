import { ITextAreaProps, TextArea } from 'components/form';
import { useFormikContext } from 'formik';

import * as styled from './FormikTextAreaStyled';

export interface IFormikTextAreaProps extends ITextAreaProps {
  name: string;
  label?: string;
  value?: string | number | readonly string[];
}

export const FormikTextArea = <T,>({
  id,
  name,
  label,
  value,
  className,
  onChange,
  onBlur,
  ...rest
}: IFormikTextAreaProps) => {
  const { values, errors, touched, handleBlur, handleChange } = useFormikContext<T>();
  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  return (
    <styled.FormikTextArea>
      {label && <label htmlFor={id ?? `txa-${name}`}>{label}</label>}
      <div>
        <TextArea
          id={id ?? `txa-${name}`}
          name={name}
          value={value ?? (values as any)[name] ?? ''}
          onChange={onChange ?? handleChange}
          onBlur={onBlur ?? handleBlur}
          className={error ? `${className} error` : className}
          {...rest}
        ></TextArea>
        {error ? <p role="alert">{error}</p> : null}
      </div>
    </styled.FormikTextArea>
  );
};

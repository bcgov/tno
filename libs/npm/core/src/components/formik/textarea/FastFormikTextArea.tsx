import { ITextAreaProps, TextArea } from 'components/form/textarea';
import { FormikProps, getIn } from 'formik';
import React from 'react';
import { memo } from 'react';

import { formikFieldMemo } from '../utils';
import * as styled from './styled';

export interface IFastFormikTextAreaProps extends ITextAreaProps {
  value?: string | number | readonly string[];
  formikProps: FormikProps<any>;
}

export const FastFormikTextArea: React.FC<IFastFormikTextAreaProps> = memo(
  ({
    id,
    name,
    value,
    className,
    onChange,
    onBlur,
    formikProps: {
      values,
      errors,
      touched,
      handleChange,
      handleBlur,
      setFieldValue,
      registerField,
      unregisterField,
    },
    ...rest
  }) => {
    const errorMessage = getIn(errors, name);
    const error = errorMessage && getIn(touched, name) && errorMessage;
    const fieldValue = getIn(values, name);

    React.useEffect(() => {
      registerField(name, { validate: undefined });
      return () => {
        unregisterField(name);
      };
    }, [name, registerField, unregisterField]);

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
  },
  formikFieldMemo,
);

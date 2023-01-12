import { FormikProps, getIn } from 'formik';
import React from 'react';
import { memo } from 'react';
import { ITextProps, Text, TextVariant } from 'tno-core';

import { formikFieldMemo } from '../utils';
import * as styled from './styled';

export interface IFastFormikTextProps extends ITextProps {
  /** formik state used for context and memo calculations */
  formikProps: FormikProps<any>;
}

/**
 * FormikText component provides a formik controlled text input.
 * If you override `onBlur` or `onChange` you will need to possibly handle the formik integration.
 * @param param0 Component properties.
 * @returns
 */
export const FastFormikText: React.FC<IFastFormikTextProps> = memo(
  ({
    id,
    name,
    value,
    className,
    disabled,
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
      <styled.FormikText>
        <Text
          id={id ?? `txt-${name}`}
          name={name}
          error={error}
          value={value ?? fieldValue ?? ''}
          onChange={onChange ?? handleChange}
          onBlur={onBlur ?? handleBlur}
          className={error ? `${className ?? ''} error` : className}
          disabled={disabled}
          variant={disabled ? TextVariant.disabled : TextVariant.primary}
          {...rest}
        ></Text>
      </styled.FormikText>
    );
  },
  formikFieldMemo,
);

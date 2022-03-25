import { IOptionItem } from 'components/form/options';
import { ISelectProps, Select } from 'components/form/select';
import { useFormikContext } from 'formik';
import React from 'react';
import { ActionMeta } from 'react-select';

import * as styled from './styled';

export interface IFormikSelectProps<OptionType> extends ISelectProps<OptionType> {
  options: OptionType[];
}

export const FormikSelect = <OptionType extends IOptionItem>({
  name,
  isDisabled,
  options,
  onChange,
  ...rest
}: IFormikSelectProps<OptionType>) => {
  const { values, errors, touched, handleBlur, isSubmitting, setFieldValue } =
    useFormikContext<IFormikSelectProps<OptionType>>();

  const error = (errors as any)[name] && (touched as any)[name] && (errors as any)[name];
  const value = options.find((option) => option.value === (values as any)[name]);

  return (
    <styled.FormikSelect>
      <Select
        name={name}
        value={value}
        options={options}
        // TODO: Figure out how to strongly type these values.
        onChange={(newValue: unknown, actionMeta: ActionMeta<unknown>) => {
          if (onChange) onChange(newValue, actionMeta);
          else {
            const option = newValue as OptionType;
            setFieldValue(name, option?.value);
          }
        }}
        onBlur={handleBlur}
        isDisabled={isDisabled || isSubmitting}
        error={error}
        {...rest}
      />
    </styled.FormikSelect>
  );
};

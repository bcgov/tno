import { getIn, useFormikContext } from 'formik';
import { ActionMeta } from 'react-select';

import { IOptionItem } from '../../form/options';
import { ISelectProps, Select } from '../../form/select';

export interface IFormikSelectProps<OptionType> extends ISelectProps<OptionType> {
  /**
   * An array of select options.
   */
  options: OptionType[];
  /**
   * The value to set when clearing the current selection.
   */
  clearValue?: unknown;
}

export const FormikSelect = <OptionType extends IOptionItem>({
  name,
  isDisabled,
  options,
  onChange,
  clearValue = undefined,
  ...rest
}: IFormikSelectProps<OptionType>) => {
  const { values, errors, touched, handleBlur, isSubmitting, setFieldValue } =
    useFormikContext<IFormikSelectProps<OptionType>>();

  const errorMessage = getIn(errors, name);
  const error = errorMessage && getIn(touched, name) && errorMessage;
  const value = values
    ? options.find((option) => option.value === (values as any)[name])
    : undefined;

  return (
    <Select
      name={name}
      value={value}
      options={options}
      // TODO: Figure out how to strongly type these values.
      onChange={(newValue: unknown, actionMeta: ActionMeta<unknown>) => {
        const option = newValue as OptionType;
        setFieldValue(name, option?.value);
        if (onChange) onChange(newValue, actionMeta);
      }}
      onBlur={handleBlur}
      clearValue={Array.isArray(value) ? [] : clearValue}
      isDisabled={isDisabled || isSubmitting}
      error={error}
      {...rest}
    />
  );
};

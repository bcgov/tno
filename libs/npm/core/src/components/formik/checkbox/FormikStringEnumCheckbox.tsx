import { getIn, useFormikContext } from 'formik';

import { FormikCheckbox, IFormikCheckboxProps } from '.';

export interface IFormikStringEnumCheckboxProps<TEnum> extends IFormikCheckboxProps {
  /** values must be numeric to work with bitwise logic. */
  value: any | TEnum | TEnum[];
  deliminator?: string;
}

/**
 * FormikStringEnumCheckbox provides a way to handle a string/enum array of values.
 * @param param0 Component properties.
 * @returns A FormikStringEnumCheckbox component.
 */
export const FormikStringEnumCheckbox = <TEnum extends unknown>({
  name,
  field,
  value,
  deliminator = ',',
  defaultValue,
  checked,
  onChange,
  ...rest
}: IFormikStringEnumCheckboxProps<TEnum>) => {
  const { values, setFieldValue } = useFormikContext();

  const fieldName = field ?? name;
  const originalFieldValue = getIn(values, fieldName);
  const isArray = Array.isArray(originalFieldValue);

  const fieldValue = getIn(values, fieldName, isArray ? [] : defaultValue ?? '');
  const isChecked =
    checked || (isArray ? fieldValue.some((v: any) => v === value) : fieldValue.includes(value));

  const handleChange = (e: React.ChangeEvent<any>) => {
    if (isArray) {
      let newValue = [...fieldValue];
      if (e.target.checked) newValue.push(value);
      else newValue = fieldValue.filter((v: any) => v !== value);
      setFieldValue(fieldName, newValue);
    } else {
      var options = fieldValue
        .split(deliminator)
        .map((v: any) => v.trim())
        .filter((v: any) => !!v);
      if (e.target.checked) options.push(value);
      else options = options.filter((v: any) => v !== value);
      setFieldValue(fieldName, options.join(deliminator));
    }
    if (onChange) onChange(e);
  };

  return (
    <FormikCheckbox
      name={name}
      field={fieldName}
      value={value}
      checked={isChecked}
      onChange={handleChange}
      {...rest}
    />
  );
};

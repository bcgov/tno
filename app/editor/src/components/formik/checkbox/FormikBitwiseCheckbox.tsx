import { getIn, useFormikContext } from 'formik';

import { FormikCheckbox, IFormikCheckboxProps } from '.';

export interface IFormikBitwiseCheckbox extends IFormikCheckboxProps {
  /** values must be numeric to work with bitwise logic. */
  value: any | number;
  /** Whether the value should be stored as an array. */
  isArray?: boolean;
}

/**
 * A component to display bitwise flag values.
 * This component has two ways of saving values, the default is as an integer.
 * If the field value is an array it will automatically save as an array as long as the default value is an empty array.
 * You can inform it that it is an array if required with `isArray=true`.
 * @param param0 Component properties.
 * @returns A FormikBitwiseCheckbox component.
 */
export const FormikBitwiseCheckbox: React.FC<IFormikBitwiseCheckbox> = ({
  name,
  field,
  value,
  isArray,
  checked,
  onChange,
  ...rest
}) => {
  const { values, setFieldValue } = useFormikContext();

  const fieldName = field ?? name;
  const originalFieldValue = getIn(values, fieldName);
  isArray = isArray || Array.isArray(originalFieldValue);
  const fieldValue = getIn(values, fieldName, isArray ? [] : 0);
  const isChecked =
    checked || isArray ? fieldValue.some((v: any) => v === value) : (fieldValue & value) === value;

  const handleChange = (e: React.ChangeEvent<any>) => {
    if (isArray) {
      let newValue = [...fieldValue];
      if (e.target.checked) newValue.push(value);
      else newValue = fieldValue.filter((v: any) => v !== value);
      setFieldValue(fieldName, newValue);
    } else {
      setFieldValue(fieldName, e.target.checked ? fieldValue | value : fieldValue - value);
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

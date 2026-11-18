import { getIn, useFormikContext } from 'formik';
import React, { InputHTMLAttributes } from 'react';

export interface IFormikHiddenProps extends InputHTMLAttributes<HTMLInputElement> {
  /**
   * Form field name and field assessor.
   */
  name: string;
}

export const FormikHidden: React.FC<IFormikHiddenProps> = ({ name, value, onChange, ...rest }) => {
  const { values, handleChange } = useFormikContext();
  const fieldValue = getIn(values, name);

  return (
    <input
      type="hidden"
      name={name}
      value={value ?? fieldValue}
      onChange={(e) => {
        handleChange(e);
        if (onChange) onChange(e);
      }}
      {...rest}
    />
  );
};

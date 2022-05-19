import { IOptionItem, IRadioGroupProps, RadioGroup, RadioVariant } from 'components/form';
import { getIn, useFormikContext } from 'formik';

export interface IFormikRadioGroupProps<
  OT extends string | number | IOptionItem | HTMLOptionElement,
> extends IRadioGroupProps<OT> {}

export const FormikRadioGroup = <T, OT extends string | number | IOptionItem | HTMLOptionElement>({
  id,
  name,
  value,
  className,
  disabled,
  onChange,
  onBlur,
  ...rest
}: IFormikRadioGroupProps<OT>) => {
  const { values, errors, touched, handleBlur, isSubmitting, setFieldValue } =
    useFormikContext<T>();

  const errorMessage = getIn(errors, name);
  const error = errorMessage && getIn(touched, name) && errorMessage;
  const fieldValue = getIn(values, name);

  return (
    <RadioGroup
      name={name}
      error={error}
      value={value ?? fieldValue ?? ''}
      onChange={
        onChange ??
        ((e, value) => {
          setFieldValue(name, value);
        })
      }
      onBlur={onBlur ?? handleBlur}
      className={error ? `${className ?? ''} error` : className}
      disabled={disabled || isSubmitting}
      variant={disabled ? RadioVariant.disabled : RadioVariant.primary}
      {...rest}
    />
  );
};

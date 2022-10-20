import { FormikProps, FormikTouched, setNestedObjectValues } from 'formik';

/**
 * When a button is not of type submit, this function can be used to force validate formik errors
 * @param {FormikProps} props - The formik props from the designated component
 * */
export const triggerFormikValidate = <T>(props: FormikProps<T>) => {
  const errors = props.errors;
  if (Object.keys(errors).length !== 0) {
    props.setTouched(setNestedObjectValues<FormikTouched<T>>(errors, true));
  }
};

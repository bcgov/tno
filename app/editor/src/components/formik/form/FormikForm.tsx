import { Formik, FormikConfig, FormikValues } from 'formik';
import React from 'react';

export type FormikFormProps<
  Values extends FormikValues = FormikValues,
  ExtraProps = {},
> = FormikConfig<Values> & ExtraProps;

export interface IFormikFormProps<Values> extends FormikFormProps<Values> {}

export const FormikForm = <Values extends FormikValues = FormikValues>({
  children,
  initialValues,
  enableReinitialize = true,
  validate,
  onSubmit,
  ...rest
}: IFormikFormProps<Values>) => {
  return (
    <Formik
      enableReinitialize={enableReinitialize}
      initialValues={initialValues}
      validate={validate}
      onSubmit={onSubmit}
      {...rest}
    >
      {(props) => (
        <form onSubmit={props.handleSubmit}>
          {typeof children === 'function' ? children(props) : children}
        </form>
      )}
    </Formik>
  );
};

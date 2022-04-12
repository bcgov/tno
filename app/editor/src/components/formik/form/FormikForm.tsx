import { Formik, FormikConfig, FormikValues } from 'formik';

import * as styled from './styled';

export type FormikFormProps<
  Values extends FormikValues = FormikValues,
  ExtraProps = {},
> = FormikConfig<Values> & ExtraProps;

export interface IFormikFormProps<Values> extends FormikFormProps<Values> {}

export const FormikForm = <Values extends FormikValues = FormikValues>({
  children,
  enableReinitialize = true,
  ...rest
}: IFormikFormProps<Values>) => {
  return (
    <styled.FormikForm>
      <Formik enableReinitialize={enableReinitialize} {...rest}>
        {(props) => (
          <form onSubmit={props.handleSubmit}>
            {typeof children === 'function' ? children(props) : children}
          </form>
        )}
      </Formik>
    </styled.FormikForm>
  );
};

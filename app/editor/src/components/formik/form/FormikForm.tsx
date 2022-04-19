import { Formik, FormikConfig, FormikValues } from 'formik';
import { useApp } from 'store/hooks';
import { Loader, SpinnerVariant } from 'tno-core';

import * as styled from './styled';

export type FormikFormProps<
  Values extends FormikValues = FormikValues,
  ExtraProps = {},
> = FormikConfig<Values> & ExtraProps;

export interface IFormikFormProps<Values> extends FormikFormProps<Values> {
  /**
   * Manually control when the loading spinner overlay is visible.
   */
  loading?: boolean;
  /**
   * Configuration options for loading spinner overlay.
   */
  load?: {
    hasRequests: boolean;
    size?: string;
    variant?: SpinnerVariant;
  };
}

export const FormikForm = <Values extends FormikValues = FormikValues>({
  loading,
  load = {
    hasRequests: true,
    size: '5em',
  },
  children,
  enableReinitialize = true,
  ...rest
}: IFormikFormProps<Values>) => {
  const [{ requests }] = useApp();

  return (
    <styled.FormikForm>
      <Loader
        size={load.size}
        variant={load.variant}
        visible={loading || (load.hasRequests && !!requests.length)}
      />
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

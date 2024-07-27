import { Form, Formik, FormikConfig, FormikValues } from 'formik';
import { useApp } from 'store/hooks';
import { Box, SpinnerVariant } from 'tno-core';

import * as styled from './styled';

export type FormikFormProps<
  Values extends FormikValues = FormikValues,
  ExtraProps = {},
> = FormikConfig<Values> & ExtraProps;

/** Provides a predicate to determine if any requests are active. */
type IsLoading = (request: any) => boolean;

export interface IFormikFormProps<Values extends FormikValues> extends FormikFormProps<Values> {
  /**
   * Manually control when the loading spinner overlay is visible.
   */
  loading?: boolean | IsLoading;
  /**
   * Configuration options for loading spinner overlay.
   */
  load?: {
    size?: string;
    variant?: SpinnerVariant;
  };
}

/**
 * Component provides a Formik form and a page loader.
 * @param param0 Component properties.
 * @returns Component.
 */
export const FormikForm = <Values extends FormikValues = FormikValues>({
  loading = (request) => !request.isSilent,
  load = {
    size: '5em',
  },
  children,
  enableReinitialize = true,
  ...rest
}: IFormikFormProps<Values>) => {
  const [{ requests }] = useApp();

  return (
    <styled.FormikForm>
      <Box isLoading={typeof loading === 'function' ? requests.some(loading) : loading}>
        <Formik enableReinitialize={enableReinitialize} {...rest}>
          {(props) => <Form>{typeof children === 'function' ? children(props) : children}</Form>}
        </Formik>
      </Box>
    </styled.FormikForm>
  );
};

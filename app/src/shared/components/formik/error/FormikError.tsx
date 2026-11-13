import { getIn, useFormikContext } from 'formik';

import { Error, IErrorProps } from '../../form/error';

export interface IFormikErrorProps extends IErrorProps {
  /** The name of the property this error is associated with. */
  name?: string;
}

/**
 * Simple component to conditionally display an error message
 * @param param0 Component properties.
 * @returns Component.
 */
export const FormikError: React.FC<IFormikErrorProps> = ({ name, error: initError }) => {
  const { errors } = useFormikContext();

  const error = !!name ? getIn(errors, name) : initError;

  return <Error error={error} />;
};

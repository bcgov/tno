import { FormikErrors, FormikTouched } from 'formik';

import { ITab } from './ITab';

export interface IFormikTab<T extends unknown> extends ITab {
  validationSchema?: any;
  validateOnChange?: boolean;
  errors?: FormikErrors<T>;
  touched?: FormikTouched<T>;
}

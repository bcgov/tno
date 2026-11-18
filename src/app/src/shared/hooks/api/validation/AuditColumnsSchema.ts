import * as Yup from 'yup';

import { IAuditColumnsModel } from '../';

/**
 * Validation schema for audit columns.
 */
export const AuditColumnsSchema: Yup.ObjectSchema<IAuditColumnsModel> = Yup.object().shape({
  createdOn: Yup.string(),
  createdBy: Yup.string(),
  createdById: Yup.string(),
  updatedOn: Yup.string(),
  updatedBy: Yup.string(),
  updatedById: Yup.string(),
  version: Yup.number(),
});

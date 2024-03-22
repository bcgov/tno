import { array, number, object, string } from 'yup';

import { ReportFormScheduleSchema } from '.';

export const ReportFormTemplateSchema = object().shape(
  {
    name: string().required('Report must have a name.'),
    templateId: number()
      .min(1, 'Report must have a template.')
      .required('Report must have a template.'),
    events: array().of(ReportFormScheduleSchema),
  },
  [],
);

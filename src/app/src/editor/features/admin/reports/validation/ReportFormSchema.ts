import { array, object, string } from 'yup';

import { ReportFormScheduleSchema } from './ReportFormScheduleSchema';

export const ReportFormSchema = object().shape(
  {
    name: string().required('Report should have a name.'),
    templateId: string().required('Report should have a template.'),
    settings: object({
      subject: object({ text: string().required('Email subject line should have a text.') }),
    }),
    events: array().of(ReportFormScheduleSchema),
  },
  [],
);

import { number, object, string } from 'yup';

export const ReportFormTemplateSchema = object().shape(
  {
    name: string().required('Report must have a name.'),
    templateId: number()
      .min(1, 'Report must have a template.')
      .required('Report must have a template.'),
  },
  [],
);

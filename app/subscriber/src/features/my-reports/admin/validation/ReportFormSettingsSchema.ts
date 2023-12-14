import { object, string } from 'yup';

export const ReportFormSettingsSchema = object().shape(
  {
    settings: object({
      subject: object({ text: string().required('Email subject line is required.') }),
    }),
  },
  [],
);

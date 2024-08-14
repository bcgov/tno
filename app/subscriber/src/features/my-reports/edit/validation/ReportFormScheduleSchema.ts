import { boolean, object, string } from 'yup';
const regex24hour = /^(?:[01][0-9]|2[0-3]):[0-5][0-9](?::[0-5][0-9])?$/;
export const ReportFormScheduleSchema = object().shape(
  {
    isEnabled: boolean(),
    runOnWeekDays: string().when('isEnabled', {
      is: true,
      then: (schema) =>
        schema.notOneOf(['NA'], 'Schedule must have at least one weekday when Enabled.'),
    }),
    startAt: string().when('isEnabled', {
      is: true,
      then: (schema) =>
        schema
          .matches(regex24hour, 'Invalid 24 hour format.')
          .required('Schedule must have a time when Enabled.'),
      otherwise: (schema) => schema.matches(regex24hour, 'Invalid 24 hour format.'),
    }),
  },
  [],
);

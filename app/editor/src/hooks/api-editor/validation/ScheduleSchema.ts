import { IScheduleModel, ScheduleTypeName } from 'hooks';
import * as Yup from 'yup';

import { AuditColumnsSchema } from './AuditColumnsSchema';

/**
 * Validation schema for base schedules.
 */
export const ScheduleSchema: Yup.SchemaOf<IScheduleModel> = AuditColumnsSchema.shape({
  id: Yup.number().defined(),
  description: Yup.string().optional() as Yup.StringSchema<string>,
  isEnabled: Yup.boolean().defined(),
  scheduleType: Yup.mixed<ScheduleTypeName>().oneOf(Object.values(ScheduleTypeName)).defined(),
  name: Yup.string()
    .required('Name is a required field')
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  delayMS: Yup.number().defined(),
  runOn: Yup.date()
    .optional()
    .default(undefined)
    .transform((curr, orig) => (!orig ? undefined : curr)),
  startAt: Yup.string().optional().default(undefined),
  stopAt: Yup.string().optional().default(undefined),
  repeat: Yup.number().defined(),
  runOnWeekDays: Yup.string().defined(),
  runOnMonths: Yup.string().defined(),
  dayOfMonth: Yup.number().defined(),
});

import * as Yup from 'yup';

import { IScheduleModel } from '../';
import { AuditColumnsSchema } from './AuditColumnsSchema';

/**
 * Validation schema for base schedules.
 */
export const ScheduleSchema: Yup.ObjectSchema<Omit<IScheduleModel, 'startAt' | 'stopAt'>> =
  AuditColumnsSchema.shape({
    id: Yup.number().defined(),
    description: Yup.string().optional() as Yup.StringSchema<string>,
    isEnabled: Yup.boolean().defined(),
    name: Yup.string()
      .required('Name is a required field')
      .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
    delayMS: Yup.number().defined('Must be defined'),
    runOn: Yup.date()
      .optional()
      .default(undefined)
      .transform((curr, orig) => (!orig ? undefined : curr)),
    runOnlyOnce: Yup.boolean().defined(),
    repeat: Yup.boolean().defined(),
    runOnWeekDays: Yup.string().defined(),
    runOnMonths: Yup.string().defined(),
    dayOfMonth: Yup.number().defined(),
    requestedById: Yup.number().optional(),
  });

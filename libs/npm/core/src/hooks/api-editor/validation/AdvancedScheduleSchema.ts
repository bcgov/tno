import * as Yup from 'yup';

import { IScheduleModel } from '../';
import { ScheduleSchema } from './ScheduleSchema';

/**
 * Validation schema for data source advanced schedule.
 */
export const AdvancedScheduleSchema: Yup.SchemaOf<IScheduleModel> = ScheduleSchema.shape({
  startAt: Yup.string().required('Start At is a required field'),
  stopAt: Yup.string().required('Stop At is a required field'),
  delayMS: Yup.number().required(),
});

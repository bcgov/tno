import { string } from 'yup';

import { ScheduleSchema } from './ScheduleSchema';

/**
 * Validation schema for data source advanced schedule.
 */
export const AdvancedScheduleSchema = ScheduleSchema.shape({
  startAt: string().required('Start At is a required field'),
  stopAt: string().required('Stop At is a required field'),
});

import { array, number, object, string } from 'yup';

import { ScheduleSchema } from './ScheduleSchema';

/**
 * Validation schema for data ingests.
 */
export const IngestSchema = object().shape({
  name: string()
    .required()
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  topic: string()
    .required()
    .test('length', 'Maximum length is 20', (val) => (val?.length ?? 0) <= 20),

  sourceId: number().integer().min(1, 'Source required').required(),
  mediaTypeId: number().integer().min(1, 'Media Type required').required(),
  productId: number().integer().min(1, 'Product required').required(),
  sourceConnectionId: number().integer().min(1, 'Source connection required').required(),
  destinationConnectionId: number().integer().min(1, 'Destination connection required').required(),
  schedules: array().of(ScheduleSchema),
});

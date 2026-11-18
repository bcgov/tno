import { array, number, object, string } from 'yup';

import { ScheduleSchema, ScheduleTypeName } from '..';

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
  ingestTypeId: number().integer().min(1, 'Ingest Type required').required(),
  mediaTypeId: number().integer().min(1, 'Media Type required').required(),
  sourceConnectionId: number().integer().min(1, 'Source connection required').required(),
  destinationConnectionId: number().integer().min(1, 'Destination connection required').required(),
  schedules: array().when('scheduleType', (value) => {
    if (value[0] === ScheduleTypeName.Daily || value[0] === ScheduleTypeName.Advanced)
      return array().of(
        ScheduleSchema.shape({
          stopAt: string().required(`Required for schedules.`),
          startAt: string().required(`Required for schedules.`),
        }),
      );
    return array().of(
      ScheduleSchema.shape({
        stopAt: string().optional().default(undefined),
        startAt: string().optional().default(undefined),
      }),
    );
  }),
  dataLocations: array()
    .min(1, 'Select at least one location to run this ingest in')
    .required('Location required'),
});

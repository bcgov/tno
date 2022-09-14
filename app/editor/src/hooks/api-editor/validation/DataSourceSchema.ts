import { array, number, object, string } from 'yup';

import { ScheduleSchema } from './ScheduleSchema';

/**
 * Validation schema for data sources.
 */
export const DataSourceSchema = object().shape({
  name: string()
    .required()
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  code: string()
    .required()
    .test('length', 'Maximum length is 20', (val) => (val?.length ?? 0) <= 20),

  mediaTypeId: number().integer().min(1, 'Media Type required').required(),
  contentTypeId: number().integer().min(0, 'Content Type required').required(), // TODO: This is conditional based on media type and connection settings.
  licenseId: number().integer().min(1, 'License required').required(),
  dataLocationId: number().integer().min(1, 'Data Location required').required(),
  schedules: array().of(ScheduleSchema),
});

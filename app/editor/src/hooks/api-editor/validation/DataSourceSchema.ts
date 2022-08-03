import { array, number, object, string, StringSchema } from 'yup';

import { ScheduleSchema } from './ScheduleSchema';

const pattern = /^[a-zA-Z0-9._-]+$/;

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
  topic: string()
    .when(['schedules'], (schedules, schema: StringSchema) => {
      return schema.test(
        'required',
        'Kafka topic is required',
        (val?: string) => !schedules.length || !!val?.length,
      );
    })
    .when(['schedules'], (schedules, schema: StringSchema) => {
      return schema.test(
        'valid',
        'Kafka topic must contain only the following characters: a-z, A-Z, 0-9, . (dot), _ (underscore), - (dash).',
        (val?: string) => !schedules.length || (!!val?.length && pattern.test(val)),
      );
    })
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  mediaTypeId: number().integer().min(1, 'Media Type required').required(),
  contentTypeId: number().integer().min(0, 'Content Type required').required(), // TODO: This is conditional based on media type and connection settings.
  licenseId: number().integer().min(1, 'License required').required(),
  dataLocationId: number().integer().min(1, 'Data Location required').required(),
  schedules: array().of(ScheduleSchema),
});

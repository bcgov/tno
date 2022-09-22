import { number, object, string } from 'yup';

/**
 * Validation schema for sources.
 */
export const SourceSchema = object().shape({
  name: string()
    .required()
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  code: string()
    .required()
    .test('length', 'Maximum length is 20', (val) => (val?.length ?? 0) <= 20),

  licenseId: number().integer().min(1, 'License required').required(),
});

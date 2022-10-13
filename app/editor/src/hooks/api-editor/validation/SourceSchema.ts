import { number, object, string } from 'yup';

/**
 * Validation schema for sources.
 */
export const SourceSchema = object().shape({
  name: string()
    .required('Legal Name is a required field.')
    .test('length', 'Maximum length is 50', (val) => (val?.length ?? 0) <= 50),
  code: string()
    .required('Code is a required field.')
    .test('length', 'Maximum length is 20', (val) => (val?.length ?? 0) <= 20),

  licenseId: number().integer().min(1, 'License is a required field').required(),
});
